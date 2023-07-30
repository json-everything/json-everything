using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `maxProperties`.
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
[JsonConverter(typeof(MaxPropertiesKeywordJsonConverter))]
public class MaxPropertiesKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "maxProperties";

	/// <summary>
	/// The maximum expected number of properties.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Creates a new <see cref="MaxPropertiesKeyword"/>.
	/// </summary>
	/// <param name="value">The maximum expected number of properties.</param>
	public MaxPropertiesKeyword(uint value)
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
		if (Value < number)
			evaluation.Results.Fail(Name, ErrorMessages.MaxProperties, ("received", number), ("limit", Value));
	}
}

internal class MaxPropertiesKeywordJsonConverter : JsonConverter<MaxPropertiesKeyword>
{
	public override MaxPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected a number");

		var number = reader.GetDecimal();
		if (number != Math.Floor(number))
			throw new JsonException("Expected an integer");
		if (number < 0)
			throw new JsonException("Expected a positive integer");

		return new MaxPropertiesKeyword((uint)number);
	}
	public override void Write(Utf8JsonWriter writer, MaxPropertiesKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MaxPropertiesKeyword.Name, value.Value);
	}
}

public static partial class ErrorMessages
{
	private static string? _maxProperties;

	/// <summary>
	/// Gets or sets the error message for <see cref="MaxPropertiesKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of properties provided in the JSON instance
	///   - [[limit]] - the upper limit specified in the schema
	/// </remarks>
	public static string MaxProperties
	{
		get => _maxProperties ?? Get();
		set => _maxProperties = value;
	}
}