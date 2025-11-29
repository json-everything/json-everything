using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `prefixItems`.
/// </summary>
/// <remarks>
/// This keyword validates that the first _n_ items of an array are valid against the first _n_ subschemas by index-matching.
/// </remarks>
public class PrefixItemsKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="PrefixItemsKeyword"/>.
	/// </summary>
	public static PrefixItemsKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "prefixItems";

	/// <summary>
	/// Initializes a new instance of the <see cref="PrefixItemsKeyword"/> class.
	/// </summary>
	protected PrefixItemsKeyword()
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
			throw new JsonSchemaException($"'{Name}' value must be an object, found {value.ValueKind}");

		if (value.EnumerateArray().Any(x => x.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False)))
			throw new JsonSchemaException("Values must be valid schemas");

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
		if (context.Instance.ValueKind != JsonValueKind.Array) return KeywordEvaluation.Ignore;

		var subschemaEvaluations = new List<EvaluationResults>();
		var pairs = keyword.Subschemas.Zip(context.Instance.EnumerateArray(), (s, i) => (s, i));

		var i = 0;
		foreach (var (subschema, instance) in pairs)
		{
			var itemContext = context with
			{
				InstanceLocation = context.InstanceLocation.Combine(i),
				Instance = instance,
				EvaluationPath = context.EvaluationPath.Combine(Name, i)
			};

			subschemaEvaluations.Add(subschema.Evaluate(itemContext));
			i++;
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluations.Count == 0 || subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray(),
			Annotation = (subschemaEvaluations.Count - 1).AsJsonElement()
		};
	}
}
