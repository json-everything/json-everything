using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `maximum`.
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
[JsonConverter(typeof(MaximumKeywordJsonConverter))]
public class MaximumKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "maximum";

	/// <summary>
	/// The maximum expected value.
	/// </summary>
	public decimal Value { get; }

	/// <summary>
	/// Creates a new <see cref="MaximumKeyword"/>.
	/// </summary>
	/// <param name="value">The maximum expected value.</param>
	public MaximumKeyword(decimal value)
	{
		Value = value;
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
		var schemaValueType = evaluation.LocalInstance.GetSchemaValueType();
		if (schemaValueType is not (SchemaValueType.Number or SchemaValueType.Integer))
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var number = evaluation.LocalInstance!.AsValue().GetNumber()!.Value;
		if (Value < number)
			evaluation.Results.Fail(Name, ErrorMessages.GetMaximum(context.Options.Culture), ("received", number), ("limit", Value));
	}
}

/// <summary>
/// JSON converter for <see cref="MaximumKeyword"/>.
/// </summary>
public sealed class MaximumKeywordJsonConverter : Json.More.AotCompatibleJsonConverter<MaximumKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="MaximumKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override MaximumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected number");

		var number = reader.GetDecimal();

		return new MaximumKeyword(number);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, MaximumKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue(value.Value);
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="MinimumKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[limit]] - the upper limit in the schema
	/// </remarks>
	public static string? Maximum { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MinimumKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[limit]] - the upper limit in the schema
	/// </remarks>
	public static string GetMaximum(CultureInfo? culture)
	{
		return Maximum ?? Get(culture);
	}
}