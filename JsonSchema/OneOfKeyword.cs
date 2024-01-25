using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `oneOf`.
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
[JsonConverter(typeof(OneOfKeywordJsonConverter))]
public class OneOfKeyword : IJsonSchemaKeyword, ISchemaCollector
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "oneOf";

	/// <summary>
	/// The keywords schema collection.
	/// </summary>
	public IReadOnlyList<JsonSchema> Schemas { get; }

	/// <summary>
	/// Creates a new <see cref="OneOfKeyword"/>.
	/// </summary>
	/// <param name="values">The keywords schema collection.</param>
	public OneOfKeyword(params JsonSchema[] values)
	{
		Schemas = values.ToReadOnlyList() ?? throw new ArgumentNullException(nameof(values));
	}

	/// <summary>
	/// Creates a new <see cref="OneOfKeyword"/>.
	/// </summary>
	/// <param name="values">The keywords schema collection.</param>
	public OneOfKeyword(IEnumerable<JsonSchema> values)
	{
		Schemas = values.ToReadOnlyList();
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
		var subschemaConstraints = Schemas.Select((x, i) => x.GetConstraint(JsonPointer.Create(Name, i), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context)).ToArray();

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = subschemaConstraints
		};
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		var actual = evaluation.ChildEvaluations.Count(x => x.Results.IsValid);
		if (actual != 1)
		{
			evaluation.Results.SetAnnotation(Name, actual);
			evaluation.Results.Fail(Name, ErrorMessages.GetOneOf(context.Options.Culture), ("count", actual));
		}
	}
}

/// <summary>
/// JSON converter for <see cref="OneOfKeyword"/>.
/// </summary>
public sealed class OneOfKeywordJsonConverter : JsonConverter<OneOfKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="OneOfKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override OneOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected array");

		var schemas = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.ListJsonSchema)!;
		return new OneOfKeyword(schemas);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, OneOfKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var schema in value.Schemas)
		{
			JsonSerializer.Serialize(writer, schema, options);
		}
		writer.WriteEndArray();
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="OneOfKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[count]] - the number of subschemas that passed validation
	/// </remarks>
	public static string? OneOf { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="OneOfKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[count]] - the number of subschemas that passed validation
	/// </remarks>
	public static string GetOneOf(CultureInfo? culture)
	{
		return OneOf ?? Get(culture);
	}
}