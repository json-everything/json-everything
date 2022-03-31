﻿using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `maxLength`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[JsonConverter(typeof(MaxLengthKeywordJsonConverter))]
public class MaxLengthKeyword : IJsonSchemaKeyword, IEquatable<MaxLengthKeyword>
{
	internal const string Name = "maxLength";

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
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		if (context.LocalInstance.ValueKind != JsonValueKind.String)
		{
			context.LocalResult.Pass();
			context.WrongValueKind(context.LocalInstance.ValueKind);
			return;
		}

		var length = new StringInfo(context.LocalInstance.GetString()).LengthInTextElements;
		if (Value >= length)
			context.LocalResult.Pass();
		else
			context.LocalResult.Fail($"Value is not shorter than or equal to {Value} characters");
		context.ExitKeyword(Name, context.LocalResult.IsValid);
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
			throw new JsonException("Expected number");

		var number = reader.GetUInt32();

		return new MaxLengthKeyword(number);
	}
	public override void Write(Utf8JsonWriter writer, MaxLengthKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MaxLengthKeyword.Name, value.Value);
	}
}