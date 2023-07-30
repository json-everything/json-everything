using System;
using System.Collections.Generic;
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
			evaluation.Results.Fail(Name, ErrorMessages.MinProperties, ("received", number), ("limit", Value));
	}
}

internal class MinPropertiesKeywordJsonConverter : JsonConverter<MinPropertiesKeyword>
{
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
	public override void Write(Utf8JsonWriter writer, MinPropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MinPropertiesKeyword.Name, value.Value);
	}
}

public static partial class ErrorMessages
{
	private static string? _minProperties;

	/// <summary>
	/// Gets or sets the error message for <see cref="MinPropertiesKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of properties provided in the JSON instance
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string MinProperties
	{
		get => _minProperties ?? Get();
		set => _minProperties = value;
	}
}