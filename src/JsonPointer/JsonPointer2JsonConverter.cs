using System;
using System.Text.Json;
using Json.More;

namespace Json.Pointer;

/// <summary>
/// Converter for <see cref="JsonPointer2"/>.
/// </summary>
public sealed class JsonPointer2JsonConverter : WeaklyTypedJsonConverter<JsonPointer2>
{
    /// <summary>Reads and converts the JSON to type <see cref="JsonPointer2"/>.</summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>The converted value.</returns>
    public override JsonPointer2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected string");

        var str = reader.GetString()!;
        return JsonPointer2.Parse(str);
    }

    /// <summary>Writes a specified value as JSON.</summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, JsonPointer2 value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
} 