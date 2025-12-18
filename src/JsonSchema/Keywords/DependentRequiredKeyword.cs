using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `dependentRequired`.
/// </summary>
/// <remarks>
/// This keyword specifies that if a property is present in an object, then other properties must also be present.
/// The value of this keyword is an object where each key is a property name, and each value is an array of
/// strings representing the required properties.
/// </remarks>
public class DependentRequiredKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="DependentRequiredKeyword"/>.
	/// </summary>
	public static DependentRequiredKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "dependentRequired";

	/// <summary>
	/// Initializes a new instance of the <see cref="DependentRequiredKeyword"/> class.
	/// </summary>
	protected DependentRequiredKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
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

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
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

				if (context.CanOptimize)
				{
					return new KeywordEvaluation
					{
						Keyword = Name,
						IsValid = false
					};
				}

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
