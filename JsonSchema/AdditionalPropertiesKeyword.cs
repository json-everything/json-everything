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
/// Handles `additionalProperties`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[DependsOnAnnotationsFrom(typeof(PropertiesKeyword))]
[DependsOnAnnotationsFrom(typeof(PatternPropertiesKeyword))]
[JsonConverter(typeof(AdditionalPropertiesKeywordJsonConverter))]
public class AdditionalPropertiesKeyword : IJsonSchemaKeyword, ISchemaContainer
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "additionalProperties";

	/// <summary>
	/// The schema by which to evaluate additional properties.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="AdditionalPropertiesKeyword"/>.
	/// </summary>
	/// <param name="value">The keyword's schema.</param>
	public AdditionalPropertiesKeyword(JsonSchema value)
	{
		Schema = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	///     The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	///     Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, ReadOnlySpan<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		var propertiesConstraint = localConstraints.GetKeywordConstraint<PropertiesKeyword>();
		var patternPropertiesConstraint = localConstraints.GetKeywordConstraint<PatternPropertiesKeyword>();
		var keywordConstraints = new[] { propertiesConstraint, patternPropertiesConstraint }.Where(x => x != null).ToArray();

		var subschemaConstraint = Schema.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);
		subschemaConstraint.InstanceLocator = LocateInstances;

		return new KeywordConstraint(Name, Evaluator)
		{
			SiblingDependencies = keywordConstraints!,
			ChildDependencies = [subschemaConstraint]
		};
	}

	private static IEnumerable<JsonPointer> LocateInstances(KeywordEvaluation evaluation)
	{
		if (evaluation.LocalInstance is not JsonObject obj) yield break;

		var skip = new HashSet<string>();

		var propertiesEvaluation = evaluation.GetKeywordEvaluation<PropertiesKeyword>();
		if (propertiesEvaluation != null)
		{
			foreach (var child in propertiesEvaluation.ChildEvaluations)
			{
				skip.Add(child.RelativeInstanceLocation[0].GetSegmentName());
			}
		};

		var patternPropertiesEvaluation = evaluation.GetKeywordEvaluation<PatternPropertiesKeyword>();
		if (patternPropertiesEvaluation != null)
		{
			foreach (var child in patternPropertiesEvaluation.ChildEvaluations)
			{
				skip.Add(child.RelativeInstanceLocation[0].GetSegmentName());
			}
		}

		foreach (var kvp in obj)
		{
			if (skip.Contains(kvp.Key)) continue;

			yield return JsonPointer.Create(kvp.Key);
		}
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		evaluation.Results.SetAnnotation(Name, evaluation.ChildEvaluations.Select(x => (JsonNode)x.RelativeInstanceLocation[0].GetSegmentName()!).ToJsonArray());

		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}
}

/// <summary>
/// JSON converter for <see cref="AdditionalPropertiesKeyword"/>.
/// </summary>
public sealed class AdditionalPropertiesKeywordJsonConverter : WeaklyTypedJsonConverter<AdditionalPropertiesKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="AdditionalPropertiesKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override AdditionalPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = options.Read(ref reader, JsonSchemaSerializerContext.Default.JsonSchema)!;

		return new AdditionalPropertiesKeyword(schema);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, AdditionalPropertiesKeyword value, JsonSerializerOptions options)
	{
		options.Write(writer, value.Schema, JsonSchemaSerializerContext.Default.JsonSchema);
	}
}