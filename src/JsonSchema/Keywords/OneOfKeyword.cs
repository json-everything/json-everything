using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `oneOf`.
/// </summary>
public class OneOfKeyword : IKeywordHandler
{
	public static OneOfKeyword Instance { get; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "oneOf";

	protected OneOfKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not JsonValueKind.Array)
			throw new JsonSchemaException($"'{Name}' value must be an array, found {value.ValueKind}.");

		var count = 0;
		foreach (var x in value.EnumerateArray())
		{
			if (x.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False))
				throw new JsonSchemaException($"'{Name}' values must be valid schemas.");
			count++;
		}

		if (count == 0)
			throw new JsonSchemaException($"'{Name}' requires at least one subschema.");

		return null;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var subschemas = new List<JsonSchemaNode>();
		var index = 0;
		foreach (var definition in keyword.RawValue.EnumerateArray())
		{
			var defContext = context with
			{
				LocalSchema = definition
			};
			var node = JsonSchema.BuildNode(defContext);
			node.RelativePath = JsonPointer.Create(index);

			subschemas.Add(node);
			index++;
		}

		keyword.Subschemas = subschemas.ToArray();
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var subschemaEvaluations = new List<EvaluationResults>();

		var i = 0;
		foreach (var subschema in keyword.Subschemas)
		{
			var evaluationPath = context.EvaluationPath.Combine(i);
			var itemContext = context with
			{
				EvaluationPath = evaluationPath.Combine(Name, i)
			};

			subschemaEvaluations.Add(subschema.Evaluate(itemContext));
			i++;
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluations.Count(x => x.IsValid) == 1,
			Details = subschemaEvaluations.ToArray()
		};
	}
}
