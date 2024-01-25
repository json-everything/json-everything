using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `unevaluatedProperties`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[DependsOnAnnotationsFrom<PropertiesKeyword>]
[DependsOnAnnotationsFrom<PatternPropertiesKeyword>]
[DependsOnAnnotationsFrom<AdditionalPropertiesKeyword>]
[DependsOnAnnotationsFrom<ContainsKeyword>]
[DependsOnAnnotationsFrom<UnevaluatedPropertiesKeyword>]
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

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	/// The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	/// Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
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

/// <summary>
/// JSON converter for <see cref="UnevaluatedPropertiesKeyword"/>.
/// </summary>
public sealed class UnevaluatedPropertiesKeywordJsonConverter : JsonConverter<UnevaluatedPropertiesKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="UnevaluatedPropertiesKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override UnevaluatedPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.JsonSchema)!;

		return new UnevaluatedPropertiesKeyword(schema);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, UnevaluatedPropertiesKeyword value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}