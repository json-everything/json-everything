using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$defs`.
/// </summary>
//[SchemaKeyword(Name)]
//[SchemaSpecVersion(SpecVersion.Draft201909)]
//[SchemaSpecVersion(SpecVersion.Draft202012)]
//[SchemaSpecVersion(SpecVersion.DraftNext)]
//[Vocabulary(Vocabularies.Core201909Id)]
//[Vocabulary(Vocabularies.Core202012Id)]
//[Vocabulary(Vocabularies.CoreNextId)]
public class DefsKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$defs";

	public object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.Object)
			throw new JsonSchemaException($"'$defs' value must be an object, found {value.ValueKind}");

		if (value.EnumerateObject().Any(x => x.Value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False)))
			throw new JsonSchemaException("Values must be valid schemas");

		return null;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
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

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return KeywordEvaluation.Ignore;
	}
}
