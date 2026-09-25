using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Json.Schema.Api.Analyzer;

/// <summary>
/// Shared discovery logic for controllers and minimal APIs.
/// </summary>
internal static class EndpointDiscoveryHelpers
{
	/// <summary>
	/// Indicates whether a parameter type would be bound from the request body.
	/// </summary>
	/// <remarks>
	/// Mirrors ASP.NET's inference: simple types and well-known framework types bind from
	/// the route or query string; everything else binds from the body.
	/// </remarks>
	public static bool LooksLikeBody(ITypeSymbol type)
	{
		var unwrapped = UnwrapNullable(type);

		if (unwrapped.SpecialType != SpecialType.None) return false;
		if (unwrapped.TypeKind == TypeKind.Enum) return false;

		return unwrapped.ToDisplayString() switch
		{
			"System.Guid" or "System.DateTime" or "System.DateTimeOffset" or
			"System.TimeSpan" or "System.Uri" or "System.DateOnly" or "System.TimeOnly" => false,
			_ => !IsFrameworkService(unwrapped)
		};
	}

	/// <summary>
	/// Indicates whether a route template contains a segment that binds the named parameter.
	/// </summary>
	/// <remarks>
	/// A segment carries more than the name — `{id:int}` constrains it, `{id?}` makes it
	/// optional, and `{*rest}` catches the remainder — so the name has to be read out of the
	/// segment rather than matched against it whole.
	/// </remarks>
	public static bool RouteBinds(string route, string parameterName)
	{
		var start = 0;

		while ((start = route.IndexOf('{', start)) >= 0)
		{
			var end = route.IndexOf('}', start);
			if (end < 0) break;

			if (SegmentName(route.Substring(start + 1, end - start - 1)) == parameterName)
				return true;

			start = end + 1;
		}

		return false;
	}

	private static string SegmentName(string segment)
	{
		// `{*rest}` and `{**rest}` catch the remainder of the path.
		segment = segment.TrimStart('*');

		// A constraint follows the name, and may itself be parameterized: `{id:min(1)}`.
		var colon = segment.IndexOf(':');
		if (colon >= 0) segment = segment.Substring(0, colon);

		// `{id=5}` supplies a default.
		var equals = segment.IndexOf('=');
		if (equals >= 0) segment = segment.Substring(0, equals);

		return segment.TrimEnd('?');
	}

	/// <summary>
	/// Indicates whether a type is an ASP.NET service rather than request data.
	/// </summary>
	public static bool IsFrameworkService(ITypeSymbol type)
	{
		var name = type.ToDisplayString();

		return name.StartsWith("Microsoft.AspNetCore.") ||
			   name.StartsWith("Microsoft.Extensions.") ||
			   name == "System.Threading.CancellationToken";
	}

	/// <summary>
	/// Reads the responses declared by `[ProducesResponseType]` attributes.
	/// </summary>
	public static List<EndpointResponseInfo> GetDeclaredResponses(IMethodSymbol method)
	{
		var responses = new List<EndpointResponseInfo>();

		foreach (var attribute in method.GetAttributes())
		{
			if (attribute.AttributeClass?.Name != "ProducesResponseTypeAttribute") continue;

			var statusCode = 200;
			string? typeName = null;

			foreach (var arg in attribute.ConstructorArguments)
			{
				if (arg.Value is int code) statusCode = code;
				else if (arg.Value is INamedTypeSymbol type)
					typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			}

			if (attribute.AttributeClass!.TypeArguments.Length > 0)
				typeName = attribute.AttributeClass.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			responses.Add(new EndpointResponseInfo
			{
				StatusCode = statusCode,
				TypeName = typeName,
				Description = DescribeStatus(statusCode)
			});
		}

		return responses;
	}

	/// <summary>
	/// Recovers the payload type of a method's success response.
	/// </summary>
	/// <remarks>
	/// Declared return types are usually erased — `IActionResult`, `IResult`, and
	/// `Task&lt;IResult&gt;` carry no payload type — so this falls back to reading the
	/// argument of an `Ok(...)` call in the body.
	/// </remarks>
	public static string? GetReturnPayloadType(IMethodSymbol method)
	{
		var returnType = UnwrapTask(method.ReturnType);

		// `ActionResult<T>` and `Results<T, …>` keep the payload type.
		if (returnType is INamedTypeSymbol { IsGenericType: true } named &&
			named.Name is "ActionResult" &&
			named.TypeArguments.Length == 1)
		{
			return named.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		}

		if (!IsErasedResult(returnType))
			return returnType.SpecialType == SpecialType.System_Void
				? null
				: returnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		return null;
	}

	/// <summary>
	/// Recovers the payload type from an `Ok(...)`-style call in a method body or lambda.
	/// </summary>
	/// <param name="body">The body to inspect.</param>
	/// <param name="semanticModel">The semantic model for the body's tree.</param>
	/// <returns>The fully-qualified payload type name, or null.</returns>
	public static string? GetPayloadTypeFromBody(SyntaxNode? body, SemanticModel semanticModel)
	{
		if (body is null) return null;

		foreach (var invocation in body.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
		{
			if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol symbol) continue;
			if (symbol.Name != "Ok") continue;

			var containing = symbol.ContainingType?.ToDisplayString();
			if (containing is not ("Microsoft.AspNetCore.Http.Results" or
								   "Microsoft.AspNetCore.Http.TypedResults" or
								   "Microsoft.AspNetCore.Mvc.ControllerBase")) continue;

			// `TypedResults.Ok<T>(…)`, and the generic `Results.Ok<T>(…)` overload, carry
			// the payload type directly.
			if (symbol.TypeArguments.Length == 1)
				return symbol.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			if (invocation.ArgumentList.Arguments.Count == 0) continue;

			// Otherwise read the argument's natural type.  The non-generic overloads take
			// `object?`, so the converted type would always be `object`.
			var type = semanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression).Type;

			if (type is null || type.SpecialType == SpecialType.System_Object) continue;

			return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		}

		return null;
	}

	private static bool IsResultFactory(InvocationExpressionSyntax invocation, SemanticModel semanticModel, out ExpressionSyntax? argument)
	{
		argument = null;

		if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol symbol) return false;
		if (symbol.Name != "Ok") return false;

		var containing = symbol.ContainingType?.ToDisplayString();
		if (containing is not ("Microsoft.AspNetCore.Http.Results" or
							   "Microsoft.AspNetCore.Http.TypedResults" or
							   "Microsoft.AspNetCore.Mvc.ControllerBase")) return false;

		if (symbol.TypeArguments.Length == 1)
		{
			// `TypedResults.Ok<T>(…)` carries the type directly.
			argument = null;
			return false;
		}

		if (invocation.ArgumentList.Arguments.Count == 0) return false;

		argument = invocation.ArgumentList.Arguments[0].Expression;
		return true;
	}

	/// <summary>
	/// Indicates whether a return type carries no payload information.
	/// </summary>
	public static bool IsErasedResult(ITypeSymbol type) =>
		type.ToDisplayString() is "Microsoft.AspNetCore.Mvc.IActionResult" or
								  "Microsoft.AspNetCore.Http.IResult" or
								  "Microsoft.AspNetCore.Mvc.ActionResult";

	/// <summary>
	/// Unwraps `Task&lt;T&gt;` and `ValueTask&lt;T&gt;`.
	/// </summary>
	public static ITypeSymbol UnwrapTask(ITypeSymbol type)
	{
		if (type is not INamedTypeSymbol { IsGenericType: true } named) return type;

		return named.ConstructedFrom.ToDisplayString() switch
		{
			"System.Threading.Tasks.Task<TResult>" or
			"System.Threading.Tasks.ValueTask<TResult>" => named.TypeArguments[0],
			_ => type
		};
	}

	private static ITypeSymbol UnwrapNullable(ITypeSymbol type)
	{
		if (type is INamedTypeSymbol { IsGenericType: true } named &&
			named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
		{
			return named.TypeArguments[0];
		}

		return type;
	}

	/// <summary>
	/// Produces a description for a status code.
	/// </summary>
	public static string DescribeStatus(int statusCode) => statusCode switch
	{
		200 => "Success",
		201 => "Created",
		202 => "Accepted",
		204 => "No content",
		400 => "Bad request",
		401 => "Unauthorized",
		403 => "Forbidden",
		404 => "Not found",
		409 => "Conflict",
		422 => "Unprocessable content",
		500 => "Server error",
		_ => $"Response {statusCode}"
	};
}
