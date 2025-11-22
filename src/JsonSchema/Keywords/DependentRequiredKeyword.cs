using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `dependentRequired`.
/// </summary>
public class DependentRequiredKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "dependentRequired";

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.Object)
			throw new JsonSchemaException($"'{Name}' value must be an object, found {value.ValueKind}");

		foreach (var x in value.EnumerateObject())
		{
			if (x.Value.ValueKind is not JsonValueKind.Array ||
			    x.Value.EnumerateArray().Any(y => y.ValueKind is not JsonValueKind.String))
				throw new JsonSchemaException("Values must be arrays of strings");
		}

		var required = value
			.EnumerateObject()
			.ToDictionary(x => x.Name, x => x.Value.EnumerateArray().Select(y => y.GetString()!).ToArray());

		return required;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (context.Instance.ValueKind is not JsonValueKind.Object) return KeywordEvaluation.Ignore;

		HashSet<string>? missing = null;

		var required = (Dictionary<string, string[]>)keyword.Value!;
		foreach (var property in required)
		{
			if (!context.Instance.TryGetProperty(property.Key, out _)) continue;

			foreach (var requiredProperty in property.Value)
			{
				if (context.Instance.TryGetProperty(requiredProperty, out _)) continue;

				missing ??= [];
				missing.Add(requiredProperty);
			}
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
			Error = ErrorMessages.GetDependentRequired(context.Options.Culture)
				.ReplaceToken("missing", missing, JsonSchemaSerializerContext.Default.HashSetString)
		};
	}
}
