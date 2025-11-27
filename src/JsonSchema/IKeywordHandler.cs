using System.Text.Json;

namespace Json.Schema;

public interface IKeywordHandler
{
	string Name { get; }

	object? ValidateKeywordValue(JsonElement value);
	void BuildSubschemas(KeywordData keyword, BuildContext context);
	KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context);
}