using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.JsonE.Tests.Suite;

[JsonConverter(typeof(TestSuiteConverter))]
public class TestSuite
{
#pragma warning disable CS8618
	public List<Test> Tests { get; set; }
#pragma warning restore CS8618
}

public class TestSuiteConverter : JsonConverter<TestSuite?>
{
	public override TestSuite? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Test suite must be an array of tests.");

		var tests = JsonSerializer.Deserialize<List<Test>>(ref reader, options)!
			.Where(t => t != null)
			.ToList();

		return new TestSuite { Tests = tests };
	}

	public override void Write(Utf8JsonWriter writer, TestSuite? value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}