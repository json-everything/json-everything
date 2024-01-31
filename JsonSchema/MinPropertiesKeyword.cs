using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `minProperties`.
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
[JsonConverter(typeof(MinPropertiesKeywordJsonConverter))]
public class MinPropertiesKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "minProperties";

	/// <summary>
	/// The minimum expected number of properties.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Creates a new <see cref="MinPropertiesKeyword"/>.
	/// </summary>
	/// <param name="value">The minimum expected number of properties.</param>
	public MinPropertiesKeyword(uint value)
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
		if (evaluation.LocalInstance is not JsonObject obj)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var number = obj.Count;
		if (Value > number)
			evaluation.Results.Fail(Name, ErrorMessages.GetMinProperties(context.Options.Culture)
				.ReplaceToken("received", number)
				.ReplaceToken("limit", Value));
	}
}

/// <summary>
/// JSON converter for <see cref="MinPropertiesKeyword"/>.
/// </summary>
public sealed class MinPropertiesKeywordJsonConverter : Json.More.AotCompatibleJsonConverter<MinPropertiesKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="MinPropertiesKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override MinPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected a number");

		var number = reader.GetDecimal();
		if (number != Math.Floor(number))
			throw new JsonException("Expected an integer");
		if (number < 0)
			throw new JsonException("Expected a positive integer");

		return new MinPropertiesKeyword((uint)number);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, MinPropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue(value.Value);
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="MinPropertiesKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of properties provided in the JSON instance
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string? MinProperties { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="MinPropertiesKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of properties provided in the JSON instance
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string GetMinProperties(CultureInfo? culture)
	{
		return MinProperties ?? Get(culture);
	}
}