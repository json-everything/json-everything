using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Schema.Experiments;

public class FormatKeywordHandler : IKeywordHandler
{
	private readonly bool _assert;

	public static FormatKeywordHandler Annotate { get; } = new(false);
	public static FormatKeywordHandler Assert { get; } = new(true);

	public string Name => "format";
	public string[]? Dependencies { get; }

	private FormatKeywordHandler(bool assert)
	{
		_assert = assert;
	}

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> siblingEvaluations)
	{
		var formatKey = (keywordValue as JsonValue)?.GetString();
		if (formatKey is null)
			throw new SchemaValidationException("format must contain a string", context);

		if (!_assert && !context.Options.RequireFormatValidation) return KeywordEvaluation.Annotate;

		var format = Formats.Get(formatKey);
		if (format is null) return KeywordEvaluation.Annotate;

		return format.Validate(context.LocalInstance, out _);
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}
