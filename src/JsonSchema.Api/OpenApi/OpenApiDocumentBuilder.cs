using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Assembles the fragments emitted across an application's assemblies into one document.
/// </summary>
internal static class OpenApiDocumentBuilder
{
	private const string _fragmentTypeName = "GeneratedOpenApiFragment";
	private const string _validationErrorType = "https://json-everything.net/errors/validation";

	/// <summary>
	/// Collects every registered fragment.
	/// </summary>
	public static IReadOnlyList<OpenApiFragment> CollectFragments() =>
		OpenApiFragmentRegistry.GetFragments();

	/// <summary>
	/// Builds a document from the given fragments.
	/// </summary>
	/// <remarks>
	/// The title and version are seeded from the entry assembly; edit
	/// <see cref="OpenApiDocument.Info"/> to change them.
	/// </remarks>
	public static OpenApiDocument Build(IReadOnlyList<OpenApiFragment> fragments)
	{
		var name = Assembly.GetEntryAssembly()?.GetName();
		var title = name?.Name ?? "API";
		var version = name?.Version?.ToString(3) ?? "1.0.0";

		var document = new OpenApiDocument("3.1.1", new OpenApiInfo(title, version));

		var componentNames = new Dictionary<Type, string>();
		var schemas = new Dictionary<string, JsonSchema>();

		foreach (var fragment in fragments)
		{
			foreach (var kvp in fragment.ComponentNames)
			{
				componentNames[kvp.Key] = kvp.Value;
			}

			foreach (var kvp in fragment.Schemas)
			{
				schemas[kvp.Key] = kvp.Value;
			}
		}

		if (schemas.Count != 0)
			document.Components = new ComponentCollection { Schemas = schemas };

		var paths = new PathCollection();

		foreach (var operation in fragments.SelectMany(x => x.Operations).OrderBy(x => x.Route))
		{
			AddOperation(paths, operation, componentNames);
		}

		if (paths.Count != 0)
			document.Paths = paths;

		return document;
	}

	private static void AddOperation(
		PathCollection paths,
		OpenApiFragmentOperation source,
		IReadOnlyDictionary<Type, string> componentNames)
	{
		PathTemplate template = source.Route;

		if (!paths.TryGetValue(template, out var pathItem))
		{
			pathItem = new PathItem();
			paths[template] = pathItem;
		}

		var operation = new Operation
		{
			OperationId = source.OperationId,
			Parameters = BuildParameters(source, componentNames),
			RequestBody = BuildRequestBody(source, componentNames),
			Responses = BuildResponses(source, componentNames)
		};

		switch (source.Method)
		{
			case "get": pathItem.Get = operation; break;
			case "put": pathItem.Put = operation; break;
			case "post": pathItem.Post = operation; break;
			case "delete": pathItem.Delete = operation; break;
			case "patch": pathItem.Patch = operation; break;
			case "head": pathItem.Head = operation; break;
			case "options": pathItem.Options = operation; break;
			case "trace": pathItem.Trace = operation; break;
		}
	}

	private static IReadOnlyList<Parameter>? BuildParameters(
		OpenApiFragmentOperation source,
		IReadOnlyDictionary<Type, string> componentNames)
	{
		if (source.Parameters.Count == 0) return null;

		return
		[
			.. source.Parameters.Select(x => new Parameter(x.Name, ParseLocation(x.Location))
			{
				Required = x.Required,
				Schema = SchemaFor(x.Type, componentNames)
			})
		];
	}

	private static RequestBody? BuildRequestBody(
		OpenApiFragmentOperation source,
		IReadOnlyDictionary<Type, string> componentNames)
	{
		if (source.RequestBodyType is null) return null;

		return new RequestBody(new Dictionary<string, MediaType>
		{
			["application/json"] = new() { Schema = SchemaFor(source.RequestBodyType, componentNames) }
		})
		{
			Required = true
		};
	}

	private static ResponseCollection BuildResponses(
		OpenApiFragmentOperation source,
		IReadOnlyDictionary<Type, string> componentNames)
	{
		var responses = new ResponseCollection();

		foreach (var response in source.Responses)
		{
			var built = new Response(response.Description);

			var schema = SchemaFor(response.Type, componentNames);
			if (schema is not null)
			{
				built.Content = new Dictionary<string, MediaType>
				{
					["application/json"] = new() { Schema = schema }
				};
			}

			responses[(HttpStatusCode)response.StatusCode] = built;
		}

		// The validation middleware answers a malformed body with problem details before
		// the handler runs, so the response exists whether or not the handler declares it.
		if (source.RequestBodyIsValidated && !responses.ContainsKey(HttpStatusCode.BadRequest))
			responses[HttpStatusCode.BadRequest] = BuildValidationErrorResponse();

		return responses;
	}

	private static Response BuildValidationErrorResponse() =>
		new("The request body did not satisfy its schema.")
		{
			Content = new Dictionary<string, MediaType>
			{
				["application/problem+json"] = new()
				{
					Schema = new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Properties(
							("type", new JsonSchemaBuilder().Type(SchemaValueType.String).Const(_validationErrorType)),
							("title", new JsonSchemaBuilder().Type(SchemaValueType.String)),
							("status", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
							("detail", new JsonSchemaBuilder().Type(SchemaValueType.String)),
							("errors", new JsonSchemaBuilder().Type(SchemaValueType.Object))
						)
				}
			}
		};

	private static JsonSchema? SchemaFor(Type? type, IReadOnlyDictionary<Type, string> componentNames)
	{
		if (type is null) return null;

		return componentNames.TryGetValue(type, out var componentName)
			? Ref.To.Schema(componentName)
			: PrimitiveSchemaFor(type);
	}

	private static JsonSchema? PrimitiveSchemaFor(Type type)
	{
		var unwrapped = Nullable.GetUnderlyingType(type) ?? type;

		if (unwrapped == typeof(string)) return new JsonSchemaBuilder().Type(SchemaValueType.String);
		if (unwrapped == typeof(bool)) return new JsonSchemaBuilder().Type(SchemaValueType.Boolean);
		if (unwrapped == typeof(Guid)) return new JsonSchemaBuilder().Type(SchemaValueType.String).Format(global::Json.Schema.Formats.Uuid);
		if (unwrapped == typeof(DateTime) || unwrapped == typeof(DateTimeOffset))
			return new JsonSchemaBuilder().Type(SchemaValueType.String).Format(global::Json.Schema.Formats.DateTime);

		if (unwrapped == typeof(int) || unwrapped == typeof(long) ||
			unwrapped == typeof(short) || unwrapped == typeof(byte))
		{
			return new JsonSchemaBuilder().Type(SchemaValueType.Integer);
		}

		if (unwrapped == typeof(double) || unwrapped == typeof(float) || unwrapped == typeof(decimal))
			return new JsonSchemaBuilder().Type(SchemaValueType.Number);

		return null;
	}

	private static ParameterLocation ParseLocation(string location) => location switch
	{
		"path" => ParameterLocation.Path,
		"header" => ParameterLocation.Header,
		"cookie" => ParameterLocation.Cookie,
		_ => ParameterLocation.Query
	};


}
