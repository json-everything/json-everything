using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Path;

/// <summary>
/// JSON converter for <see cref="JsonPath"/>.
/// </summary>
public class JsonPathConverter : JsonConverter<JsonPath>
{
	/// <summary>Reads and converts the JSON to type <see cref="JsonPath"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override JsonPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var text = reader.GetString()!;
		return JsonPath.Parse(text);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, JsonPath value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}

[JsonSerializable(typeof(JsonPath))]
internal partial class JsonPathSerializerContext : JsonSerializerContext
{

}