using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `uniqueItems`.
/// </summary>
public class UniqueItemsKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "uniqueItems";

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		return value.ValueKind switch
		{
			JsonValueKind.True => true,
			JsonValueKind.False => false,
			_ => throw new JsonSchemaException($"'{Name}' value must be a boolean, found {value.ValueKind}")
		};
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind != JsonValueKind.Array) return KeywordEvaluation.Ignore;
		if (keyword.RawValue.ValueKind == JsonValueKind.False) return KeywordEvaluation.Ignore;

		var valid = true;
		var items = context.Instance.EnumerateArray().ToArray();
		List<string>? matches = null;
		for (int i = 0; i < items.Length; i++)
		{
			for (int j = i + 1; j < items.Length; j++)
			{
				if (items[i].IsEquivalentTo(items[j]))
				{
					valid = false;
					matches ??= [];
					matches.Add($"({i},{j})");
				}
			}
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = valid,
			Error = valid
				? null
				: ErrorMessages.GetUniqueItems(context.Options.Culture)
					.ReplaceToken("duplicates", matches!.ToArray())
		};
	}
}
