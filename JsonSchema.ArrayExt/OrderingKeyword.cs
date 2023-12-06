using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Represents the `ordering` keyword.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.ArrayExtId)]
[JsonConverter(typeof(OrderingKeywordJsonConverter))]
public class OrderingKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "ordering";

	/// <summary>
	/// The collection of keywords and references.
	/// </summary>
	public IEnumerable<OrderingSpecifier> Specifiers { get; }

	/// <summary>
	/// Creates an instance of the <see cref="OrderingKeyword"/> class.
	/// </summary>
	/// <param name="specifiers">The collection of keywords and references.</param>
	public OrderingKeyword(IEnumerable<OrderingSpecifier> specifiers)
	{
		Specifiers = specifiers;
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
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (evaluation.LocalInstance is not JsonArray array) return;

		throw new NotImplementedException();
	}
}

/// <summary>
/// JSON converter for <see cref="OrderingKeyword"/>.
/// </summary>
public sealed class OrderingKeywordJsonConverter : JsonConverter<OrderingKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="OrderingKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override OrderingKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected array");

		var references = JsonSerializer.Deserialize<List<OrderingSpecifier>>(ref reader, options)!;
		return new OrderingKeyword(references);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, OrderingKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(OrderingKeyword.Name);
		JsonSerializer.Serialize(writer, value.Specifiers, options);
	}
}