using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `minimum`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[JsonConverter(typeof(MinimumKeywordJsonConverter))]
public class MinimumKeyword : IJsonSchemaKeyword, IEquatable<MinimumKeyword>
{
	internal const string Name = "minimum";

	/// <summary>
	/// The minimum expected value.
	/// </summary>
	public decimal Value { get; }

	/// <summary>
	/// Creates a new <see cref="MinimumKeyword"/>.
	/// </summary>
	/// <param name="value">The minimum expected value.</param>
	public MinimumKeyword(decimal value)
	{
		Value = value;
	}

	/// <summary>
	/// Provides validation for the keyword.S
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType is not (SchemaValueType.Number or SchemaValueType.Integer))
		{
			context.LocalResult.Pass();
			context.WrongValueKind(schemaValueType);
			return;
		}

		var number = context.LocalInstance!.AsValue().GetNumber();
		if (Value <= number)
			context.LocalResult.Pass();
		else
			context.LocalResult.Fail(Name, ErrorMessages.Minimum, ("received", number), ("limit", Value));
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(MinimumKeyword? other)
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
		return Equals(obj as MinimumKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}

internal class MinimumKeywordJsonConverter : JsonConverter<MinimumKeyword>
{
	public override MinimumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new JsonException("Expected number");

		var number = reader.GetDecimal();

		return new MinimumKeyword(number);
	}
	public override void Write(Utf8JsonWriter writer, MinimumKeyword value, JsonSerializerOptions options)
	{
		writer.WriteNumber(MinimumKeyword.Name, value.Value);
	}
}

public static partial class ErrorMessages
{
	private static string? _minimum;

	/// <summary>
	/// Gets or sets the error message for <see cref="MinimumKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[limit]] - the lower limit in the schema
	/// </remarks>
	public static string Minimum
	{
		get => _minimum ?? Get();
		set => _minimum = value;
	}
}