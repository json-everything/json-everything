using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Tests.Suite
{
	public class EmbeddedDataJsonConverter : JsonConverter<JsonDocument>
	{
		public override JsonDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return JsonDocument.ParseValue(ref reader);
		}

		public override void Write(Utf8JsonWriter writer, JsonDocument value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}