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
[DependsOnAnnotationsFrom<PropertiesKeyword>]
[DependsOnAnnotationsFrom<PatternPropertiesKeyword>]
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
	/// The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	/// Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		var propertiesConstraint = localConstraints.FirstOrDefault(x => x.Keyword == PropertiesKeyword.Name);
		var patternPropertiesConstraint = localConstraints.FirstOrDefault(x => x.Keyword == PatternPropertiesKeyword.Name);
		var keywordConstraints = new[] { propertiesConstraint, patternPropertiesConstraint }.Where(x => x != null).ToArray()!;

		var subschemaConstraint = Schema.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);
		subschemaConstraint.InstanceLocator = evaluation =>
		{
			if (evaluation.LocalInstance is not JsonObject obj) return Array.Empty<JsonPointer>();

			var properties = obj.Select(x => x.Key);
			
			var propertiesEvaluation = evaluation.GetKeywordEvaluation<PropertiesKeyword>();
			if (propertiesEvaluation != null)
				properties = properties.Except(propertiesEvaluation.ChildEvaluations.Select(x => x.RelativeInstanceLocation.Segments[0].Value));

			var patternPropertiesEvaluation = evaluation.GetKeywordEvaluation<PatternPropertiesKeyword>();
			if (patternPropertiesEvaluation != null)
				properties = properties.Except(patternPropertiesEvaluation.ChildEvaluations.Select(x => x.RelativeInstanceLocation.Segments[0].Value));

			return properties.Select(x => JsonPointer.Create(x));
		};

		return new KeywordConstraint(Name, Evaluator)
		{
			SiblingDependencies = keywordConstraints!,
			ChildDependencies = new[] { subschemaConstraint }
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		evaluation.Results.SetAnnotation(Name, evaluation.ChildEvaluations.Select(x => (JsonNode)x.RelativeInstanceLocation.Segments[0].Value!).ToJsonArray());

		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}
}

/// <summary>
/// JSON converter for <see cref="AdditionalPropertiesKeyword"/>.
/// </summary>
public sealed class AdditionalPropertiesKeywordJsonConverter : JsonConverter<AdditionalPropertiesKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="AdditionalPropertiesKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override AdditionalPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = options.Read<JsonSchema>(ref reader)!;

		return new AdditionalPropertiesKeyword(schema);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, AdditionalPropertiesKeyword value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}