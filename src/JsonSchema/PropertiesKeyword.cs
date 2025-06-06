﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `properties`.
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
[JsonConverter(typeof(PropertiesKeywordJsonConverter))]
public class PropertiesKeyword : IJsonSchemaKeyword, IKeyedSchemaCollector
{
	private readonly Dictionary<string, JsonPointer> _evaluationPointers;
	private readonly Dictionary<string, JsonPointer> _instancePointers;

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "properties";

	/// <summary>
	/// The property schemas.
	/// </summary>
	public IReadOnlyDictionary<string, JsonSchema> Properties { get; }

	IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => Properties;

	/// <summary>
	/// Creates a new <see cref="PropertiesKeyword"/>.
	/// </summary>
	/// <param name="values">The property schemas.</param>
	public PropertiesKeyword(IReadOnlyDictionary<string, JsonSchema> values)
	{
		Properties = values ?? throw new ArgumentNullException(nameof(values));

		_evaluationPointers = values.ToDictionary(x => x.Key, x => JsonPointer.Create(Name, x.Key));
		_instancePointers = values.ToDictionary(x => x.Key, x => JsonPointer.Create(x.Key));
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
		var subschemaConstraints = Properties.Select(x =>
		{
			context.PushEvaluationPath(x.Key);
			var constraint = x.Value.GetConstraint(_evaluationPointers[x.Key], schemaConstraint.BaseInstanceLocation, _instancePointers[x.Key], context);
			context.PopEvaluationPath();
			return constraint;
		}).ToArray();

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = subschemaConstraints
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		evaluation.Results.SetAnnotation(Name, evaluation.ChildEvaluations.Select(x => (JsonNode)x.RelativeInstanceLocation[0]).ToJsonArray());

		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}
}

/// <summary>
/// JSON converter for <see cref="PropertiesKeyword"/>.
/// </summary>
public sealed class PropertiesKeywordJsonConverter : WeaklyTypedJsonConverter<PropertiesKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="PropertiesKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override PropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var schema = options.ReadDictionary(ref reader, JsonSchemaSerializerContext.Default.JsonSchema)!;
		return new PropertiesKeyword(schema);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, PropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var kvp in value.Properties)
		{
			writer.WritePropertyName(kvp.Key);
			options.Write(writer, kvp.Value, JsonSchemaSerializerContext.Default.JsonSchema);
		}
		writer.WriteEndObject();
	}
}