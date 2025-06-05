﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `allOf`.
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
[JsonConverter(typeof(AllOfKeywordJsonConverter))]
public class AllOfKeyword : IJsonSchemaKeyword, ISchemaCollector
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "allOf";

	/// <summary>
	/// The keywords schema collection.
	/// </summary>
	public IReadOnlyList<JsonSchema> Schemas { get; }

	/// <summary>
	/// Creates a new <see cref="AllOfKeyword"/>.
	/// </summary>
	/// <param name="values">The set of schemas.</param>
	public AllOfKeyword(params JsonSchema[] values)
	{
		if (values.Length == 0)
			throw new ArgumentException($"'{Name}' requires at least one subschema");

		Schemas = values.ToReadOnlyList() ?? throw new ArgumentNullException(nameof(values));
	}

	/// <summary>
	/// Creates a new <see cref="AllOfKeyword"/>.
	/// </summary>
	/// <param name="values">The set of schemas.</param>
	public AllOfKeyword(IEnumerable<JsonSchema> values)
	{
		Schemas = values.ToReadOnlyList() ?? throw new ArgumentNullException(nameof(values));

		if (Schemas.Count == 0)
			throw new ArgumentException($"'{Name}' requires at least one subschema");
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
		var subschemaConstraints = Schemas.Select((x, i) =>
		{
			context.PushEvaluationPath(i);
			var constraint = x.GetConstraint(JsonPointer_Old.Create(Name, i), schemaConstraint.BaseInstanceLocation, JsonPointer_Old.Empty, context);
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
		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}
}

/// <summary>
/// JSON converter for <see cref="AllOfKeyword"/>.
/// </summary>
public sealed class AllOfKeywordJsonConverter : WeaklyTypedJsonConverter<AllOfKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="AllOfKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override AllOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected array");

		var schemas = options.ReadList(ref reader, JsonSchemaSerializerContext.Default.JsonSchema)!;
		return new AllOfKeyword(schemas);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, AllOfKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var schema in value.Schemas)
		{
			options.Write(writer, schema, JsonSchemaSerializerContext.Default.JsonSchema);
		}
		writer.WriteEndArray();
	}
}