using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$dynamicRef`.
/// </summary>
public class DynamicRefKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$dynamicRef";

	public virtual object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException($"'$dynamicRef' value must be a string, found {value.ValueKind}");

		var anchor = value.GetString()!;
		if (!DynamicAnchorKeyword.AnchorPattern.IsMatch(anchor))
			throw new JsonSchemaException($"'$dynamicAnchor' value must match '{DynamicAnchorKeyword.AnchorPattern}'");

		return anchor;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	internal virtual void TryResolve(KeywordData keyword, BuildContext context)
	{
		// nothing to do for v1 $dynamicRef
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var anchor = (string)keyword.Value!;
		var subschema = context.BuildOptions.SchemaRegistry.Get(context.Scope, anchor);
		if (subschema is null)
			throw new RefResolutionException(context.Scope.LocalScope, anchor, true);

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
