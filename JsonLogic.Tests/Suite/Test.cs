using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Tests.Suite
{
	[JsonConverter(typeof(TestConverter))]
	public class Test
	{
		public string Logic { get; set; }
		public JsonElement Data { get; set; }
		public JsonElement Expected { get; set; }
	}

	public class TestConverter : JsonConverter<Test>
	{
		public override Test Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var element = JsonDocument.ParseValue(ref reader).RootElement;
			if (element.ValueKind != JsonValueKind.Array) return null;
			
			var items = element.EnumerateArray().ToList();
			var logic = items[0].ToJsonString();
			var data = items[1];
			var expected = items[2];
			
			return new Test
			{
				Logic = logic,
				Data = data,
				Expected = expected
			};
		}

		public override void Write(Utf8JsonWriter writer, Test value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
