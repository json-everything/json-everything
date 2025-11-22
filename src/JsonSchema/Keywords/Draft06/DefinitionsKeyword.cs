using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `$defs`.
/// </summary>
public class DefinitionsKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "definitions";

	public virtual object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.Object)
			throw new JsonSchemaException($"'{Name}' value must be an object, found {value.ValueKind}");

		if (value.EnumerateObject().Any(x => x.Value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False)))
			throw new JsonSchemaException("Values must be valid schemas");

		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var subschemas = new List<JsonSchemaNode>();
		foreach (var definition in keyword.RawValue.EnumerateObject())
		{
			var defContext = context with
			{
				LocalSchema = definition.Value
			};
			var node = JsonSchema.BuildNode(defContext);
			node.RelativePath = JsonPointer.Create(definition.Name);
			subschemas.Add(node);
		}

		keyword.Subschemas = subschemas.ToArray();
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return KeywordEvaluation.Ignore;
	}
}
