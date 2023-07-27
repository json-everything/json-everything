using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `maxLength`.
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
[JsonConverter(typeof(MaxLengthKeywordJsonConverter))]
public class MaxLengthKeyword : IJsonSchemaKeyword, IEquatable<MaxLengthKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "maxLength";

	/// <summary>
	/// The maximum expected string length.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Creates a new <see cref="MaxLengthKeyword"/>.
	/// </summary>
	/// <param name="value">The maximum expected string length.</param>
	public MaxLengthKeyword(uint value)
	{
		Value = value;
	}

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.String)
		{
			context.WrongValueKind(schemaValueType);
			return;
		}

		var length = new StringInfo(context.LocalInstance!.GetValue<string>()).LengthInTextElements;
		if (Value < length)
			context.LocalResult.Fail(Name, ErrorMessages.MaxLength, ("received", length), ("limit", Value));
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context)
	{
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation)
	{
		var schemaValueType = evaluation.LocalInstance.GetSchemaValueType();
		if (schemaValueType is not SchemaValueType.String)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var str = evaluation.LocalInstance!.GetValue<string>();
		var length = new StringInfo(str).LengthInTextElements;
		if (Value < length)
			evaluation.Results.Fail(Name, ErrorMessages.MaxLength, ("received", length), ("limit", Value));
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(MaxLengthKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Value == other.Value;
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as MaxLengthKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return (int)Value;
	}
}

internal class MaxLengthKeywordJsonConverter : JsonConverter<MaxLengthKeyword>
{
	public override MaxLengthKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected a number");

		var number = reader.GetDecimal();
		if (number != Math.Floor(number))
			throw new JsonException("Expected an integer");
		if (number < 0)
			throw new JsonException("Expected a positive integer");

		return new MaxLengthKeyword((uint)number);
	}
	public override void Write(Utf8JsonWriter writer, MaxLengthKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MaxLengthKeyword.Name, value.Value);
	}
}

public static partial class ErrorMessages
{
	private static string? _maxLength;

	/// <summary>
	/// Gets or sets the error message for <see cref="MaxLengthKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the length of the JSON string
	///   - [[limit]] - the upper limit specified in the schema
	/// </remarks>
	public static string MaxLength
	{
		get => _maxLength ?? Get();
		set => _maxLength = value;
	}
}