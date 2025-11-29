using System;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$schema`.
/// </summary>
public class VocabularyKeyword : IKeywordHandler
{
	public static VocabularyKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$vocabulary";

	protected VocabularyKeyword()
	{
	}

	public object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.Object)
			throw new JsonSchemaException($"'{Name}' requires a object.");

		foreach (var entry in value.EnumerateObject())
		{
			var uri = entry.Name;
			var required = entry.Value;

			if (!Uri.TryCreate(uri, UriKind.Absolute, out _))
				throw new JsonSchemaException($"'{Name}' keys must be absolute URIs.");
			if (required.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
				throw new JsonSchemaException($"'{Name}' values must be booleans.");
		}

		return null;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return KeywordEvaluation.Ignore;
	}
}
