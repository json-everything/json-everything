using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Pointer
{
	public class JsonPointerJsonConverter : JsonConverter<JsonPointer>
	{
		public override JsonPointer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();
			return JsonPointer.TryParse(str, out var pointer)
				? pointer
				: JsonPointer.Parse("/something/definitely/not/in/test/data");
		}

		public override void Write(Utf8JsonWriter writer, JsonPointer value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.Source);
		}
	}
}