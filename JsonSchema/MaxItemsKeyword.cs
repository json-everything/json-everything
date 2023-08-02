using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `maxItems`.
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
[JsonConverter(typeof(MaxItemsKeywordJsonConverter))]
public class MaxItemsKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "maxItems";

	/// <summary>
	/// The expected maximum number of items.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Creates a new <see cref="MaxItemsKeyword"/>.
	/// </summary>
	/// <param name="value">The expected maximum number of items.</param>
	public MaxItemsKeyword(uint value)
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
		if (evaluation.LocalInstance is not JsonArray array)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var number = array.Count;
		if (Value < number)
			evaluation.Results.Fail(Name, ErrorMessages.MaxItems, ("received", number), ("limit", Value));
	}
}

internal class MaxItemsKeywordJsonConverter : JsonConverter<MaxItemsKeyword>
{
	public override MaxItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected a number");

		var number = reader.GetDecimal();
		if (number != Math.Floor(number))
			throw new JsonException("Expected an integer");
		if (number < 0)
			throw new JsonException("Expected a positive integer");

		return new MaxItemsKeyword((uint)number);
	}
	public override void Write(Utf8JsonWriter writer, MaxItemsKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MaxItemsKeyword.Name, value.Value);
	}
}

public static partial class ErrorMessages
{
	private static string? _maxItems;

	/// <summary>
	/// Gets or sets the error message for <see cref="MaxItemsKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of items provided in the JSON instance
	///   - [[limit]] - the upper limit specified in the schema
	/// </remarks>
	public static string MaxItems
	{
		get => _maxItems ?? Get();
		set => _maxItems = value;
	}
}