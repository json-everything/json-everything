using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles the `contains` keyword.
/// </summary>
/// <remarks>
/// This keyword specifies a schema that must be met by at least one of the items in an array.
/// The `minContains` and `maxContains` keywords can be used to further constrain the number of items
/// that must match the `contains` schema.
/// </remarks>
public class ContainsKeyword : IKeywordHandler
{
	private struct ContainsLimits
	{
		public int Min { get; init; }
		public int? Max { get; init; }
	}

	/// <summary>
	/// Gets the singleton instance of the <see cref="ContainsKeyword"/>.
	/// </summary>
	public static ContainsKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "contains";

	/// <summary>
	/// Initializes a new instance of the <see cref="ContainsKeyword"/> class.
	/// </summary>
	protected ContainsKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False))
			throw new JsonSchemaException($"'{Name}' value must be a valid schema, found {value.ValueKind}");

		return null;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var defContext = context with
		{
			LocalSchema = keyword.RawValue
		};

		var node = JsonSchema.BuildNode(defContext);
		keyword.Subschemas = [node];

		var limits = new ContainsLimits
		{
			Min = context.LocalSchema.TryGetProperty("minContains", out var minContains) ? (int) Math.Truncate(minContains.GetDouble()) : 1,
			Max = context.LocalSchema.TryGetProperty("maxContains", out var maxContains) ? (int)Math.Truncate(maxContains.GetDouble()) : null
		};
		keyword.Value = limits;
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

		var limits = (ContainsLimits)keyword.Value!;

		var subschemaEvaluations = new List<EvaluationResults>();
		var found = new List<int>();
		var subschema = keyword.Subschemas[0];

		var i = 0;
		foreach (var instance in context.Instance.EnumerateArray())
		{
			var itemContext = context with
			{
				InstanceLocation = context.InstanceLocation.Combine(i),
				Instance = instance,
				EvaluationPath = context.EvaluationPath.Combine(Name)
			};

			var result = subschema.Evaluate(itemContext);
			subschemaEvaluations.Add(result);
			if (result.IsValid)
				found.Add(i);
			i++;
		}

		var valid = true;
		string? error = null;
		if (limits.Min > found.Count)
		{
			valid = false;
			error = ErrorMessages.GetContainsTooFew(context.Options.Culture)
				.ReplaceToken("received", found.Count)
				.ReplaceToken("minimum", limits.Min);
		}
		else if (found.Count > limits.Max)
		{
			valid = false;
			error = ErrorMessages.GetContainsTooMany(context.Options.Culture)
				.ReplaceToken("received", found.Count)
				.ReplaceToken("maximum", limits.Max.Value);
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = valid,
			Details = subschemaEvaluations.ToArray(),
			Error = error,
			Annotation = JsonSerializer.SerializeToElement(found.ToArray(), JsonSchemaSerializerContext.Default.Int32Array)
		};
	}
}
