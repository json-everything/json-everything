using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `anyOf`.
/// </summary>
/// <remarks>
/// This keyword specifies that the instance must be valid against at least one of the subschemas.
/// </remarks>
public class AnyOfKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="AnyOfKeyword"/>.
	/// </summary>
	public static AnyOfKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "anyOf";

	/// <summary>
	/// Initializes a new instance of the <see cref="AnyOfKeyword"/> class.
	/// </summary>
	protected AnyOfKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
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

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
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

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
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
			IsValid = subschemaEvaluations.Count == 0 || subschemaEvaluations.Any(x => x.IsValid),
			Details = subschemaEvaluations.ToArray()
		};
	}
}