using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `multipleOf`.
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
[JsonConverter(typeof(MultipleOfKeywordJsonConverter))]
public class MultipleOfKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "multipleOf";

	/// <summary>
	/// The expected divisor of a value.
	/// </summary>
	public decimal Value { get; }

	/// <summary>
	/// Creates a new <see cref="MultipleOfKeyword"/>.
	/// </summary>
	/// <param name="value">The expected divisor of a value.</param>
	public MultipleOfKeyword(decimal value)
	{
		Value = value;
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context)
	{
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, ConstraintBuilderContext context)
	{
		var schemaValueType = evaluation.LocalInstance.GetSchemaValueType();
		if (schemaValueType is not (SchemaValueType.Number or SchemaValueType.Integer))
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var number = evaluation.LocalInstance!.AsValue().GetNumber();
		if (number % Value != 0)
			evaluation.Results.Fail(Name, ErrorMessages.MultipleOf, ("received", number), ("divisor", Value));
	}
}

internal class MultipleOfKeywordJsonConverter : JsonConverter<MultipleOfKeyword>
{
	public override MultipleOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected number");

		var number = reader.GetDecimal();

		return new MultipleOfKeyword(number);
	}
	public override void Write(Utf8JsonWriter writer, MultipleOfKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MultipleOfKeyword.Name, value.Value);
	}
}

public static partial class ErrorMessages
{
	private static string? _multipleOf;

	/// <summary>
	/// Gets or sets the error message for <see cref="MultipleOfKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[divisor]] - the required divisor
	/// </remarks>
	public static string MultipleOf
	{
		get => _multipleOf ?? Get();
		set => _multipleOf = value;
	}
}