using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `minContains`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[Vocabulary(Vocabularies.ValidationNextId)]
[JsonConverter(typeof(MinContainsKeywordJsonConverter))]
public class MinContainsKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "minContains";

	/// <summary>
	/// The minimum expected matching items.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Creates a new <see cref="MinContainsKeyword"/>.
	/// </summary>
	/// <param name="value">The minimum expected matching items.</param>
	public MinContainsKeyword(uint value)
	{
		Value = value;
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(Name, (e, _) => e.Results.SetAnnotation(Name, Value));
	}
}

internal class MinContainsKeywordJsonConverter : JsonConverter<MinContainsKeyword>
{
	public override MinContainsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected a number");

		var number = reader.GetDecimal();
		if (number != Math.Floor(number))
			throw new JsonException("Expected an integer");
		if (number < 0)
			throw new JsonException("Expected a positive integer");

		return new MinContainsKeyword((uint)number);
	}
	public override void Write(Utf8JsonWriter writer, MinContainsKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MinContainsKeyword.Name, value.Value);
	}
}
