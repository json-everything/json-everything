using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Pointer
{
	internal class RelativeJsonPointerJsonConverter : JsonConverter<RelativeJsonPointer>
	{
		public override RelativeJsonPointer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();
			return RelativeJsonPointer.TryParse(str, out var pointer)
				? pointer
				: RelativeJsonPointer.Parse("0/something/definitely/not/in/test/data");
		}

		public override void Write(Utf8JsonWriter writer, RelativeJsonPointer value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}