using System;
using System.Collections.Generic;
using System.Globalization;
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
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		Span<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		var schemaValueType = evaluation.LocalInstance.GetSchemaValueType();
		if (schemaValueType is not (SchemaValueType.Number or SchemaValueType.Integer))
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var number = evaluation.LocalInstance!.AsValue().GetNumber()!.Value;
		if (number % Value != 0)
			evaluation.Results.Fail(Name, ErrorMessages.GetMultipleOf(context.Options.Culture)
				.ReplaceToken("received", number)
				.ReplaceToken("divisor", Value));
	}
}

/// <summary>
/// JSON converter for <see cref="MultipleOfKeyword"/>.
/// </summary>
public sealed class MultipleOfKeywordJsonConverter : WeaklyTypedJsonConverter<MultipleOfKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="MultipleOfKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override MultipleOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected number");

		var number = reader.GetDecimal();

		return new MultipleOfKeyword(number);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, MultipleOfKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue(value.Value);
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="MultipleOfKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[divisor]] - the required divisor
	/// </remarks>
	public static string? MultipleOf { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MultipleOfKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[divisor]] - the required divisor
	/// </remarks>
	public static string GetMultipleOf(CultureInfo? culture)
	{
		return MultipleOf ?? Get(culture);
	}
}