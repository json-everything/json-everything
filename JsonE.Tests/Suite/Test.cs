using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.JsonE.Tests.Suite;

[JsonConverter(typeof(TestConverter))]
public class Test
{
#pragma warning disable CS8618
	public string Logic { get; set; }
	public JsonNode? Data { get; set; }
	public JsonNode? Expected { get; set; }
#pragma warning restore CS8618
}

public class TestConverter : JsonConverter<Test?>
{
	public override Test? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode?>(ref reader, options);
		if (node is not JsonArray arr) return null;

		var logic = JsonSerializer.Serialize(arr[0], new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
		var data = arr[1];
		var expected = arr[2];

		return new Test
		{
			Logic = logic,
			Data = data,
			Expected = expected
		};
	}

	public override void Write(Utf8JsonWriter writer, Test? value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}