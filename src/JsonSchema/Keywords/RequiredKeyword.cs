using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `required`.
/// </summary>
public class RequiredKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "required";

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Array)
			throw new JsonSchemaException($"'{Name}' value must be an array, found {value.ValueKind}.");

		if (!value.EnumerateArray().All(x => x.ValueKind is JsonValueKind.String))
			throw new JsonSchemaException($"'{Name}' value must be an array of strings.");

		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind is not JsonValueKind.Object) return KeywordEvaluation.Ignore;

		HashSet<string>? missing = null;

		var required = keyword.RawValue.EnumerateArray().Select(x => x.GetString()!);
		foreach (var requiredProperty in required)
		{
			if (context.Instance.TryGetProperty(requiredProperty, out _)) continue;

			missing ??= [];
			missing.Add(requiredProperty);
		}

		if (missing is null or { Count: 0 })
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = false,
			Error = ErrorMessages.GetRequired(context.Options.Culture)
				.ReplaceToken("missing", missing, JsonSchemaSerializerContext.Default.HashSetString)
		};
	}
}
