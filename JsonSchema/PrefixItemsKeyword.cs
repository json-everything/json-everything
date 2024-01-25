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
/// Handles `items`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(PrefixItemsKeywordJsonConverter))]
public class PrefixItemsKeyword : IJsonSchemaKeyword, ISchemaCollector
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "prefixItems";

	/// <summary>
	/// The collection of schemas for the "schema array" form.
	/// </summary>
	public IReadOnlyList<JsonSchema> ArraySchemas { get; }

	IReadOnlyList<JsonSchema> ISchemaCollector.Schemas => ArraySchemas;

	/// <summary>
	/// Creates a new <see cref="PrefixItemsKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of schemas for the "schema array" form.</param>
	/// <remarks>
	/// Using the `params` constructor to build an array-form `items` keyword with a single schema
	/// will confuse the compiler.  To achieve this, you'll need to explicitly specify the array.
	/// </remarks>
	public PrefixItemsKeyword(params JsonSchema[] values)
	{
		ArraySchemas = values.ToReadOnlyList();
	}

	/// <summary>
	/// Creates a new <see cref="PrefixItemsKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of schemas for the "schema array" form.</param>
	public PrefixItemsKeyword(IEnumerable<JsonSchema> values)
	{
		ArraySchemas = values.ToReadOnlyList();
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
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		var subschemaConstraints = ArraySchemas.Select((x, i) => x.GetConstraint(JsonPointer.Create(Name, i), schemaConstraint.BaseInstanceLocation, JsonPointer.Create(i), context)).ToArray();

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = subschemaConstraints
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (evaluation.LocalInstance is not JsonArray array)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		if (evaluation.ChildEvaluations.Length == array.Count)
			evaluation.Results.SetAnnotation(Name, true);
		else
			evaluation.Results.SetAnnotation(Name, evaluation.ChildEvaluations.Length - 1);

		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}
}

/// <summary>
/// JSON converter for <see cref="PrefixItemsKeyword"/>.
/// </summary>
public sealed class PrefixItemsKeywordJsonConverter : JsonConverter<PrefixItemsKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="PrefixItemsKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override PrefixItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected array");

		var schemas = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.ListJsonSchema)!;
		return new PrefixItemsKeyword(schemas);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, PrefixItemsKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var schema in value.ArraySchemas)
		{
			JsonSerializer.Serialize(writer, schema, options);
		}
		writer.WriteEndArray();
	}
}