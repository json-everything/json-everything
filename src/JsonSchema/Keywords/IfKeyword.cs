using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `if`.
/// </summary>
public class IfKeyword : IKeywordHandler
{
	public static IfKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "if";

	protected IfKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False))
			throw new JsonSchemaException($"'{Name}' values must be valid schemas.");

		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var defContext = context with
		{
			LocalSchema = keyword.RawValue
		};

		var node = JsonSchema.BuildNode(defContext);
		keyword.Subschemas = [node];
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var subschema = keyword.Subschemas[0];
		var oneOfContext = context with
		{
			EvaluationPath = context.EvaluationPath.Combine(Name)
		};
		var result = subschema.Evaluate(oneOfContext);

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = result.IsValid,
			Details = [result],
			ContributesToValidation = false
		};
	}
}