using System;
using System.Text.Json;

namespace Json.More
{
	public static class Utf8JsonWriterExtensions
	{
		public static void WriteValue(this Utf8JsonWriter writer, JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Object:
					WriteObject(writer, element);
					break;
				case JsonValueKind.Array:
					WriteArray(writer, element);
					break;
				case JsonValueKind.String:
					writer.WriteStringValue(element.GetString());
					break;
				case JsonValueKind.Number:
					writer.WriteNumberValue(element.GetDecimal());
					break;
				case JsonValueKind.True:
					writer.WriteBooleanValue(true);
					break;
				case JsonValueKind.False:
					writer.WriteBooleanValue(false);
					break;
				case JsonValueKind.Null:
					writer.WriteNullValue();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static void WriteObject(this Utf8JsonWriter writer, JsonElement element)
		{
			writer.WriteStartObject();
			foreach (var property in element.EnumerateObject())
			{
				writer.WritePropertyName(property.Name);
				WriteValue(writer, property.Value);
			}
			writer.WriteEndObject();
		}

		public static void WriteArray(this Utf8JsonWriter writer, JsonElement element)
		{
			writer.WriteStartArray();
			foreach (var item in element.EnumerateArray())
			{
				WriteValue(writer, item);
			}
			writer.WriteEndArray();
		}
	}
}