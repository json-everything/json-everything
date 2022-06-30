using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `maxContains`.
/// </summary>
[SchemaPriority(10)]
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[JsonConverter(typeof(MaxContainsKeywordJsonConverter))]
public class MaxContainsKeyword : IJsonSchemaKeyword, IEquatable<MaxContainsKeyword>
{
	internal const string Name = "maxContains";

	/// <summary>
	/// The maximum expected matching items.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Creates a new <see cref="MaxContainsKeyword"/>.
	/// </summary>
	/// <param name="value">The maximum expected matching items.</param>
	public MaxContainsKeyword(uint value)
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
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.Array)
		{
			context.WrongValueKind(schemaValueType);
			context.LocalResult.Pass();
			return;
		}

		if (!context.LocalResult.TryGetAnnotation(ContainsKeyword.Name, out var annotation))
		{
			context.NotApplicable(() => $"No annotations from {ContainsKeyword.Name}.");
			context.LocalResult.Pass();
			return;
		}

		context.Log(() => $"Annotation from {ContainsKeyword.Name}: {annotation.AsJsonString()}.");
		var containsCount = annotation!.AsArray().Count;
		if (Value >= containsCount)
			context.LocalResult.Pass();
		else
			context.LocalResult.Fail(Name, ErrorMessages.MaxContains, ("received", containsCount), ("limit", Value));
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(MaxContainsKeyword? other)
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
		return Equals(obj as MaxContainsKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return (int)Value;
	}
}

internal class MaxContainsKeywordJsonConverter : JsonConverter<MaxContainsKeyword>
{
	public override MaxContainsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected a number");

		var number = reader.GetDecimal();
		if (number != Math.Floor(number))
			throw new JsonException("Expected an integer");
		if (number < 0)
			throw new JsonException("Expected a positive integer");

		return new MaxContainsKeyword((uint)number);
	}
	public override void Write(Utf8JsonWriter writer, MaxContainsKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MaxContainsKeyword.Name, value.Value);
	}
}

public static partial class ErrorMessages
{
	private static string? _maxContains;

	/// <summary>
	/// Gets or sets the error message for <see cref="MaxContainsKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of matching items provided in the JSON instance
	///   - [[limit]] - the upper limit specified in the schema
	/// </remarks>
	public static string MaxContains
	{
		get => _maxContains ?? Get();
		set => _maxContains = value;
	}
}