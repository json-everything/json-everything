using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `format`.
/// </summary>
public class FormatKeyword : IKeywordHandler
{
	public static FormatKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "format";

	protected FormatKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.String))
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var formatName = keyword.RawValue.GetString()!;
		var format = Formats.Get(formatName);

		var valid = format.Validate(context.Instance, out var error);
		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = valid,
			Annotation = keyword.RawValue,
			Error = error
		};
	}
}