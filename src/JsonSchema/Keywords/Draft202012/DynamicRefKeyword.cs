using System;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords.Draft202012;

/// <summary>
/// Handles `$dynamicRef`.
/// </summary>
/// <remarks>
/// This keyword is used to create a dynamic reference to a schema.
/// </remarks>
public class DynamicRefKeyword : Json.Schema.Keywords.DynamicRefKeyword
{
	private class DynamicRefInfo
	{
		public required Uri Uri { get; set; }
		public bool IsDynamic { get; set; }
	}

	/// <summary>
	/// Gets the singleton instance of the <see cref="DynamicRefKeyword"/>.
	/// </summary>
	public new static DynamicRefKeyword Instance { get; } = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="DynamicRefKeyword"/> class.
	/// </summary>
	protected DynamicRefKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public override object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

		if (!Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException($"'{Name}' value must be a valid URI");

		return new DynamicRefInfo { Uri = uri };
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public override void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var reference = (DynamicRefInfo)keyword.Value!;
		var newUri = context.BaseUri.Resolve(reference.Uri);

		reference.Uri = newUri; // need an absolute URI for .Fragment to work
	}

	internal override bool TryResolve(KeywordData keyword, BuildContext context)
	{
		// This method only resolves static refs.
		// Occurs when (any of)
		//   - the dynamic ref has a pointer fragment
		//   - there is no dynamic anchor in the local schema resource
		var reference = (DynamicRefInfo)keyword.Value!;
		var fragment = reference.Uri.Fragment;
		var fragmentIsPointer = JsonPointer.TryParse(fragment, out var pointerFragment);
		if (!fragmentIsPointer)
		{
			// check for local dynamic anchor
			// if there is one, reference is dynamic, so we leave
			var anchorFragment = fragment[1..]; // drop #
			var dynamicAnchor = context.Options.SchemaRegistry.GetDynamic(reference.Uri, anchorFragment);
			if (dynamicAnchor is not null)
			{
				reference.IsDynamic = true;
				return true;
			}
		}

		JsonSchemaNode? targetSchema;
		if (fragmentIsPointer)
		{
			var targetBase = context.Options.SchemaRegistry.Get(reference.Uri);

			targetSchema = targetBase?.FindSubschema(pointerFragment, context);
		}
		else
		{
			var anchorFragment = fragment[1..]; // drop #
			targetSchema = context.Options.SchemaRegistry.Get(reference.Uri, anchorFragment) ??
						   // 2020-12 supports $dynamicAnchor also acting as an $anchor when $dynamicRef acts as $ref
			               context.Options.SchemaRegistry.GetDynamic(reference.Uri, anchorFragment);
		}

		if (targetSchema is not null)
		{
			keyword.Subschemas = [targetSchema];
			return true;
		}

		return false;
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
	public override KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var reference = (DynamicRefInfo)keyword.Value!;
		JsonSchemaNode? subschema;
		if (reference.IsDynamic)
		{
			var anchor = reference.Uri.Fragment[1..];
			subschema = context.SchemaRegistry.GetDynamic(context.Scope, anchor);
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
