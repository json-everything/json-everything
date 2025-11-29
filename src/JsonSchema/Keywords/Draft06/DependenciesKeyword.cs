using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Handles `dependencies`.
/// </summary>
/// <remarks>
/// This keyword defines requirements for an object instance on the condition that given properties are present.
/// If a property is present, then its value represents a requirement.  That requirement may be an array of other
/// required properties or a subschema that the instance (the full object, not the value of the property) must pass.
/// </remarks>
public class DependenciesKeyword : IKeywordHandler
{
	/// <summary>
	/// Represents a dependency, which can be either a schema or a list of required properties.
	/// </summary>
	protected struct SchemaOrPropertyList
	{
		/// <summary>
		/// The schema dependency.
		/// </summary>
		public JsonSchemaNode? Schema { get; }
		/// <summary>
		/// The property dependency.
		/// </summary>
		public string[]? Requirements { get; }

		/// <summary>
		/// Creates a new schema dependency.
		/// </summary>
		/// <param name="value">The schema.</param>
		public SchemaOrPropertyList(JsonSchemaNode value)
		{
			Schema = value;
		}

		/// <summary>
		/// Creates a new property dependency.
		/// </summary>
		/// <param name="requirements">The list of required properties.</param>
		public SchemaOrPropertyList(string[] requirements)
		{
			Requirements = requirements;
		}
	}

	/// <summary>
	/// Gets the singleton instance of the <see cref="DependenciesKeyword"/>.
	/// </summary>
	public static DependenciesKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public string Name => "dependencies";

	/// <summary>
	/// Initializes a new instance of the <see cref="DependenciesKeyword"/> class.
	/// </summary>
	protected DependenciesKeyword()
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
			if (x.Value.ValueKind is JsonValueKind.Array &&
			    !x.Value.EnumerateArray().Any(y => y.ValueKind is not JsonValueKind.String)) continue;

			if (x.Value.ValueKind is (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False)) continue;

			throw new JsonSchemaException("Values must be an arrays of strings or a valid schemas");
		}

		return null;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var dependencies = new Dictionary<string, SchemaOrPropertyList>();
		var subschemas = new List<JsonSchemaNode>();
		foreach (var property in keyword.RawValue.EnumerateObject())
		{
			if (property.Value.ValueKind is JsonValueKind.Array)
			{
				var requirements = property.Value.EnumerateArray().Select(x => x.GetString()!).ToArray();
				dependencies.Add(property.Name, new SchemaOrPropertyList(requirements));
			}
			else
			{
				var defContext = context with
				{
					LocalSchema = property.Value
				};
				var node = JsonSchema.BuildNode(defContext);
				node.RelativePath = JsonPointer.Create(property.Name);
				dependencies.Add(property.Name, new SchemaOrPropertyList(node));
				subschemas.Add(node);
			}
		}
		keyword.Value = dependencies;
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
		if (context.Instance.ValueKind is not JsonValueKind.Object) return KeywordEvaluation.Ignore;

		HashSet<string>? missing = null;

		List<EvaluationResults>? subschemaEvaluations = null;
		var dependencies = (Dictionary<string, SchemaOrPropertyList>)keyword.Value!;
		foreach (var dependency in dependencies)
		{
			if (!context.Instance.TryGetProperty(dependency.Key, out _)) continue;

			if (dependency.Value.Requirements is not null)
			{
				foreach (var requiredProperty in dependency.Value.Requirements)
				{
					if (context.Instance.TryGetProperty(requiredProperty, out _)) continue;

					missing ??= [];
					missing.Add(requiredProperty);
				}
			}
			else
			{
				var propContext = context with
				{
					EvaluationPath = context.EvaluationPath.Combine(Name, dependency.Key)
				};

				subschemaEvaluations ??= [];
				subschemaEvaluations.Add(dependency.Value.Schema!.Evaluate(propContext));
			}
		}

		if (missing is null or { Count: 0 } && (subschemaEvaluations is null || subschemaEvaluations.All(x => x.IsValid)))
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = false,
			Details = subschemaEvaluations?.ToArray(),
			Error = missing is null or { Count: 0 }
				? null
				: ErrorMessages.GetDependentRequired(context.Options.Culture)
					.ReplaceToken("missing", missing, JsonSchemaSerializerContext.Default.HashSetString)
		};
	}
}
