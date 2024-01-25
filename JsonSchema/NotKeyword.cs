using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `not`.
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
[JsonConverter(typeof(NotKeywordJsonConverter))]
public class NotKeyword : IJsonSchemaKeyword, ISchemaContainer
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "not";

	/// <summary>
	/// The schema to not match.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="NotKeyword"/>.
	/// </summary>
	/// <param name="value">The schema to not match.</param>
	public NotKeyword(JsonSchema value)
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
		var subschemaConstraint = Schema.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = new[] { subschemaConstraint }
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (evaluation.ChildEvaluations[0].Results.IsValid)
			evaluation.Results.Fail();
	}
}

/// <summary>
/// JSON converter for <see cref="NotKeyword"/>.
/// </summary>
public sealed class NotKeywordJsonConverter : JsonConverter<NotKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="NotKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override NotKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.JsonSchema)!;

		return new NotKeyword(schema);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, NotKeyword value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}