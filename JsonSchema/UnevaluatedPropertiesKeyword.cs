using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;
// ReSharper disable AccessToModifiedClosure

namespace Json.Schema;

/// <summary>
/// Handles `unevaluatedProperties`.
/// </summary>
[SchemaPriority(30)]
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[DependsOnAnnotationsFrom(typeof(PropertiesKeyword))]
[DependsOnAnnotationsFrom(typeof(PatternPropertiesKeyword))]
[DependsOnAnnotationsFrom(typeof(AdditionalPropertiesKeyword))]
[DependsOnAnnotationsFrom(typeof(ContainsKeyword))]
[DependsOnAnnotationsFrom(typeof(UnevaluatedPropertiesKeyword))]
[JsonConverter(typeof(UnevaluatedPropertiesKeywordJsonConverter))]
public class UnevaluatedPropertiesKeyword : IJsonSchemaKeyword, ISchemaContainer
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "unevaluatedProperties";

	/// <summary>
	/// The schema by which to evaluate additional properties.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="UnevaluatedPropertiesKeyword"/>.
	/// </summary>
	/// <param name="value"></param>
	public UnevaluatedPropertiesKeyword(JsonSchema value)
	{
		Schema = value ?? throw new ArgumentNullException(nameof(value));
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, ConstraintBuilderContext context)
	{
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, ConstraintBuilderContext context)
	{
		static IEnumerable<string> GetAnnotation<T>(EvaluationResults results)
			where T : IJsonSchemaKeyword
		{
			return results.GetAllAnnotations(typeof(T).Keyword())
				.SelectMany(x => x!.AsArray())
				.Select(x => x!.GetValue<string>());
		}

		if (evaluation.LocalInstance is not JsonObject obj)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var propertiesAnnotations = GetAnnotation<PropertiesKeyword>(evaluation.Results);
		var patternPropertiesAnnotations = GetAnnotation<PatternPropertiesKeyword>(evaluation.Results);
		var containsAnnotations = GetAnnotation<ContainsKeyword>(evaluation.Results);
		var additionalPropertiesAnnotations = GetAnnotation<AdditionalPropertiesKeyword>(evaluation.Results);
		var unevaluatedPropertiesAnnotations = GetAnnotation<UnevaluatedPropertiesKeyword>(evaluation.Results);
		var properties = obj.Select(x => x.Key)
			.Except(propertiesAnnotations)
			.Except(patternPropertiesAnnotations)
			.Except(containsAnnotations)
			.Except(additionalPropertiesAnnotations)
			.Except(unevaluatedPropertiesAnnotations)
			.ToArray();

		if (properties.Length == 0)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var childEvaluations = properties
			.Select(x => (Name: x, Constraint: Schema.GetConstraint(JsonPointer.Create(Name), evaluation.Results.InstanceLocation, JsonPointer.Create(x), context)))
			.Select(x => x.Constraint.BuildEvaluation(obj[x.Name], evaluation.Results.InstanceLocation.Combine(x.Name), evaluation.Results.EvaluationPath, context.Options))
			.ToArray();

		evaluation.ChildEvaluations = childEvaluations;
		foreach (var childEvaluation in childEvaluations)
		{
			childEvaluation.Evaluate(context);
		}

		evaluation.Results.SetAnnotation(Name, properties.Select(x => (JsonNode)x!).ToJsonArray());

		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}
}

internal class UnevaluatedPropertiesKeywordJsonConverter : JsonConverter<UnevaluatedPropertiesKeyword>
{
	public override UnevaluatedPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;

		return new UnevaluatedPropertiesKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, UnevaluatedPropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(UnevaluatedPropertiesKeyword.Name);
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}