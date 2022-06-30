using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `minItems`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[JsonConverter(typeof(MinItemsKeywordJsonConverter))]
public class MinItemsKeyword : IJsonSchemaKeyword, IEquatable<MinItemsKeyword>
{
	internal const string Name = "minItems";

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
			return;
		}

		var number = ((JsonArray)context.LocalInstance!).Count;
		if (Value > number)
			context.LocalResult.Fail(Name, ErrorMessages.MinItems, ("received", number), ("limit", Value));
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(MinItemsKeyword? other)
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
		return Equals(obj as MinItemsKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return (int)Value;
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