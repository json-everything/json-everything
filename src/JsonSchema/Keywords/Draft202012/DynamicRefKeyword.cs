using System;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords.Draft202012;

/// <summary>
/// Handles `$dynamicRef`.
/// </summary>
public class DynamicRefKeyword : Json.Schema.Keywords.DynamicRefKeyword
{
	private class DynamicRefInfo
	{
		public required Uri Uri { get; set; }
		public bool RefIsLocal { get; set; }
		public bool IsDynamic { get; set; }
	}

	public override object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

		if (!Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException($"'{Name}' value must be a valid URI");

		return new DynamicRefInfo { Uri = uri };
	}

	public override void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var reference = (DynamicRefInfo)keyword.Value!;
		var newUri = new Uri(context.BaseUri, reference.Uri);

		reference.Uri = newUri; // need an absolute URI for .Fragment to work
		// For URI equality see https://docs.microsoft.com/en-us/dotnet/api/system.uri.op_equality?view=netcore-3.1
		// tl;dr - URI equality doesn't consider fragments
		reference.RefIsLocal = Equals(newUri, context.BaseUri);
	}

	internal override void TryResolve(KeywordData keyword, BuildContext context)
	{
		// This method only resolves static refs.
		// Occurs when (any of)
		//   - the dynamic ref specifies a base URI
		//   - the dynamic ref has a pointer fragment
		//   - there is no dynamic anchor in the local schema resource
		var reference = (DynamicRefInfo)keyword.Value!;
		var fragment = reference.Uri.Fragment;
		var fragmentIsPointer = JsonPointer.TryParse(fragment, out var pointerFragment);
		if (reference.RefIsLocal && !fragmentIsPointer)
		{
			// check for local dynamic anchor
			// if there is one, reference is dynamic, so we leave
			var anchorFragment = fragment[1..]; // drop #
			var dynamicAnchor = context.Options.SchemaRegistry.GetDynamic(reference.Uri, anchorFragment);
			if (dynamicAnchor is not null)
			{
				reference.IsDynamic = true;
				return;
			}
		}

		JsonSchemaNode? targetSchema;
		if (fragmentIsPointer)
		{
			var targetBase = context.Options.SchemaRegistry.Get(reference.Uri);

			targetSchema = targetBase?.FindSubschema(pointerFragment, context.Options);
		}
		else
		{
			var anchorFragment = fragment[1..]; // drop #
			targetSchema = context.Options.SchemaRegistry.Get(reference.Uri, anchorFragment);
		}

		if (targetSchema is not null)
			keyword.Subschemas = [targetSchema];
	}

	public override KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var reference = (DynamicRefInfo)keyword.Value!;
		JsonSchemaNode? subschema;
		if (reference.IsDynamic)
		{
			var anchor = reference.Uri.Fragment[1..];
			subschema = context.BuildOptions.SchemaRegistry.GetDynamic(context.Scope, anchor);
			if (subschema is null)
				throw new RefResolutionException(context.Scope.LocalScope, anchor, AnchorType.Dynamic);
		}
		else
		{
			subschema = keyword.Subschemas.FirstOrDefault();
			if (subschema is null)
				throw new RefResolutionException(reference.Uri);
		}

		var refContext = context with
		{
			EvaluationPath = context.EvaluationPath.Combine(Name)
		};

		var subschemaEvaluation = subschema.Evaluate(refContext);
		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluation.IsValid,
			Details = [subschemaEvaluation]
		};
	}
}
