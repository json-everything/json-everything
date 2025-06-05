using System;
using System.Text.Json;
using Json.More;

namespace Json.Pointer;

/// <summary>
/// Converter for <see cref="JsonPointer_Old"/>.
/// </summary>
public sealed class JsonPointerJsonConverter_Old : WeaklyTypedJsonConverter<JsonPointer_Old>
{
	/// <summary>Reads and converts the JSON to type <see cref="JsonPointer_Old"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override JsonPointer_Old Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var str = reader.GetString()!;
		return JsonPointer_Old.TryParse(str, out var pointer)
			? pointer
			: throw new JsonException("Value does not represent a JSON PointerOld");
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, JsonPointer_Old value, JsonSerializerOptions options)
	{
		if (value == null)
			writer.WriteNullValue();
		else
			writer.WriteStringValue(value.ToString());
	}
}
