using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `minLength`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[Vocabulary(Vocabularies.ValidationNextId)]
[JsonConverter(typeof(MinLengthKeywordJsonConverter))]
public class MinLengthKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "minLength";

	/// <summary>
	/// The minimum expected string length.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Creates a new <see cref="MinLengthKeyword"/>.
	/// </summary>
	/// <param name="value">The minimum expected string length.</param>
	public MinLengthKeyword(uint value)
	{
		Value = value;
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		var schemaValueType = evaluation.LocalInstance.GetSchemaValueType();
		if (schemaValueType is not SchemaValueType.String)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var str = evaluation.LocalInstance!.GetValue<string>();
		var length = new StringInfo(str).LengthInTextElements;
		if (Value > length)
			evaluation.Results.Fail(Name, ErrorMessages.MinLength, ("received", length), ("limit", Value));
	}
}

internal class MinLengthKeywordJsonConverter : JsonConverter<MinLengthKeyword>
{
	public override MinLengthKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected a number");

		var number = reader.GetDecimal();
		if (number != Math.Floor(number))
			throw new JsonException("Expected an integer");
		if (number < 0)
			throw new JsonException("Expected a positive integer");

		return new MinLengthKeyword((uint)number);
	}
	public override void Write(Utf8JsonWriter writer, MinLengthKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MinLengthKeyword.Name, value.Value);
	}
}

public static partial class ErrorMessages
{
	private static string? _minLength;

	/// <summary>
	/// Gets or sets the error message for <see cref="MinLengthKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the length of the JSON string
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string MinLength
	{
		get => _minLength ?? Get();
		set => _minLength = value;
	}
}