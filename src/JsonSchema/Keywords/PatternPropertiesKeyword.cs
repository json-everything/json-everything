using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `patternProperties`.
/// </summary>
public class PatternPropertiesKeyword : IKeywordHandler
{
	public static PatternPropertiesKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "patternProperties";

	protected PatternPropertiesKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.Object)
			throw new JsonSchemaException($"'{Name}' value must be an object, found {value.ValueKind}");

		var regexes = new Dictionary<string, Regex>();

		foreach (var x in value.EnumerateObject())
		{
			if (x.Value.ValueKind is not (JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False))
				throw new JsonSchemaException("Values must be valid schemas");

			regexes.Add(x.Name, new Regex(x.Name, RegexOptions.ECMAScript | RegexOptions.Compiled));
		}

		return regexes;
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
		if (context.Instance.ValueKind != JsonValueKind.Object) return KeywordEvaluation.Ignore;

		var regexes = (Dictionary<string, Regex>)keyword.Value!;
		var subschemaEvaluations = new List<EvaluationResults>();
		var propertyNames = new HashSet<string>();

		var properties = context.Instance.EnumerateObject().ToArray();

		foreach (var subschema in keyword.Subschemas)
		{
			var pattern = subschema.RelativePath[subschema.RelativePath.SegmentCount - 1].ToString();
			var regex = regexes[pattern];

			var evaluationPath = context.EvaluationPath.Combine(Name, pattern);
			var toEvaluate = properties.Where(x => regex.IsMatch(x.Name)).ToArray();

			foreach (var property in toEvaluate)
			{
				propertyNames.Add(property.Name);

				var propContext = context with
				{
					InstanceLocation = context.InstanceLocation.Combine(property.Name),
					Instance = property.Value,
					EvaluationPath = evaluationPath
				};

				subschemaEvaluations.Add(subschema.Evaluate(propContext));
			}
		}

		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluations.Count == 0 || subschemaEvaluations.All(x => x.IsValid),
			Details = subschemaEvaluations.ToArray(),
			Annotation = JsonSerializer.SerializeToElement(propertyNames, JsonSchemaSerializerContext.Default.HashSetString)
		};
	}
}
