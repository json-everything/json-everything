using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Pointer
{
	internal class JsonPointerJsonConverter : JsonConverter<JsonPointer?>
	{
		public override JsonPointer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();
			return JsonPointer.TryParse(str, out var pointer)
				? pointer
				: throw new JsonException("Value does not represent a JSON Pointer");
		}

		public override void Write(Utf8JsonWriter writer, JsonPointer? value, JsonSerializerOptions options)
		{
			if (value == null)
				writer.WriteNullValue();
			else
				writer.WriteStringValue(value.Source);
		}
	}
}