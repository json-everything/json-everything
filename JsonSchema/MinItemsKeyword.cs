using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `minItems`.
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
[JsonConverter(typeof(MinItemsKeywordJsonConverter))]
public class MinItemsKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "minItems";

	/// <summary>
	/// The expected minimum number of items.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Create a new <see cref="MinItemsKeyword"/>.
	/// </summary>
	/// <param name="value">The expected minimum number of items.</param>
	public MinItemsKeyword(uint value)
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
		if (evaluation.LocalInstance is not JsonArray array)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var number = array.Count;
		if (Value > number)
			evaluation.Results.Fail(Name, ErrorMessages.MaxItems, ("received", number), ("limit", Value));
	}
}

internal class MinItemsKeywordJsonConverter : JsonConverter<MinItemsKeyword>
{
	public override MinItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected a number");

		var number = reader.GetDecimal();
		if (number != Math.Floor(number))
			throw new JsonException("Expected an integer");
		if (number < 0)
			throw new JsonException("Expected a positive integer");

		return new MinItemsKeyword((uint)number);
	}
	public override void Write(Utf8JsonWriter writer, MinItemsKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MinItemsKeyword.Name, value.Value);
	}
}

public static partial class ErrorMessages
{
	private static string? _minItems;

	/// <summary>
	/// Gets or sets the error message for <see cref="MinItemsKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of items provided in the JSON instance
	///   - [[limit]] - the lower limit specified in the schema
	/// </remarks>
	public static string MinItems
	{
		get => _minItems ?? Get();
		set => _minItems = value;
	}
}