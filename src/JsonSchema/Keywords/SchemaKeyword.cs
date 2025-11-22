using System;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$schema`.
/// </summary>
public class SchemaKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$schema";

	public object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String ||
		    !Uri.TryCreate(value.GetString(), UriKind.Absolute, out var uri))
			throw new JsonSchemaException($"'{Name}' requires a string in the format of an absolute URI.");

		return uri;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var uri = (Uri)keyword.Value!;
		context.Options.KeywordRegistry = uri.OriginalString switch
		{
			"http://json-schema.org/draft-06/schema#" => SchemaKeywordRegistry.Draft06,
			"http://json-schema.org/draft-07/schema#" => SchemaKeywordRegistry.Draft07,
			"https://json-schema.org/draft/2019-09/schema" => SchemaKeywordRegistry.Draft201909,
			"https://json-schema.org/draft/2020-12/schema" => SchemaKeywordRegistry.Draft202012,
			"https://json-schema.org/v1/2026" => SchemaKeywordRegistry.V1,
			_ => context.Options.KeywordRegistry
		};
	}

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return KeywordEvaluation.Ignore;
	}
}
