using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `dependencies`.
/// </summary>
public class DependenciesKeyword : IKeywordHandler
{
	protected struct SchemaOrPropertyList
	{
		public JsonSchemaNode? Schema { get; }
		public string[]? Requirements { get; }

		public SchemaOrPropertyList(JsonSchemaNode value)
		{
			Schema = value;
		}

		public SchemaOrPropertyList(string[] requirements)
		{
			Requirements = requirements;
		}
	}

	public static DependenciesKeyword Instance { get; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "dependencies";

	protected DependenciesKeyword()
	{
	}

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
