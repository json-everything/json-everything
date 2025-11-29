using System.Text.Json;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `format`.
/// </summary>
public class FormatKeyword : Json.Schema.Keywords.FormatKeyword
{
	private readonly bool _requireFormatValidation;

	public static FormatKeyword Annotate { get; set; } = new(false);
	public static FormatKeyword Validate { get; set; } = new(true);

	protected FormatKeyword(bool requireFormatValidation)
	{
		_requireFormatValidation = requireFormatValidation;
	}

	public override object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.String))
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

		return null;
	}

	public override void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public override KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Options.RequireFormatValidation || _requireFormatValidation)
			return base.Evaluate(keyword, context);

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = true,
			Annotation = keyword.RawValue
		};
	}
}