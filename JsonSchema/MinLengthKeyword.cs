using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

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
public class MinLengthKeyword : IJsonSchemaKeyword, IKeywordHandler
{
	public static MinLengthKeyword Handler { get; } = new(0);

	bool IKeywordHandler.Evaluate(FunctionalEvaluationContext context)
	{
		if (!context.LocalSchema.AsObject().TryGetValue(Name, out var requirement, out _)) return true;

		decimal? reqNumber;
		if (requirement is not JsonValue reqValue || (reqNumber = reqValue.GetInteger()) is null || reqNumber < 0)
			throw new Exception("maxLength must be a non-negative integer");

		if (context.LocalInstance is not JsonValue value) return true;

		var str = value.GetString();
		if (str is null) return true;

		var length = new StringInfo(str).LengthInTextElements;
		return length >= reqNumber;
	}

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
		if (schemaValueType is not SchemaValueType.String)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var str = evaluation.LocalInstance!.GetValue<string>();
		var length = new StringInfo(str).LengthInTextElements;
		if (Value > length)
			evaluation.Results.Fail(Name, ErrorMessages.GetMinLength(context.Options.Culture)
				.ReplaceToken("received", length)
				.ReplaceToken("limit", Value));
	}
}

/// <summary>
/// JSON converter for <see cref="MinLengthKeyword"/>.
/// </summary>
public sealed class MinLengthKeywordJsonConverter : WeaklyTypedJsonConverter<MinLengthKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="MinLengthKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
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

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, MinLengthKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue(value.Value);
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="MinLengthKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the length of the JSON string
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string? MinLength { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MinLengthKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the length of the JSON string
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string GetMinLength(CultureInfo? culture)
	{
		return MinLength ?? Get(culture);
	}
}