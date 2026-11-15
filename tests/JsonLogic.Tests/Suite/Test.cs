using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Tests.Suite;

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
		var node = options.Read(ref reader, TestSerializerContext.Default.JsonNode);
		if (node is not JsonArray arr) return null;

		var logic = JsonSerializer.Serialize(arr[0], TestEnvironment.SerializerOptions);
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