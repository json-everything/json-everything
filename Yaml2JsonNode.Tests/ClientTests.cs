using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.More;
using NUnit.Framework;

namespace Yaml2JsonNode.Tests;

public class ClientTests
{
	[Test]
	public void Issue476_RoundTripJson()
	{
		var jsonText = "{\"foo\":123,\"foz\":\"123\",\"bar\":1.23,\"baz\": \"1.23\"}";
		var json = JsonNode.Parse(jsonText)!;
		var yaml = json.ToYamlNode();
		var yamlText = YamlSerializer.Serialize(yaml: yaml);

		var jsonRoundTripped = YamlSerializer.Parse(yamlText).Single().ToJsonNode();
		var jsonRoundText = jsonRoundTripped.ToJsonString();

		Console.WriteLine("# jsonText:");
		Console.WriteLine(jsonText);
		Console.WriteLine();
		Console.WriteLine("# yamlText:");
		Console.WriteLine(yamlText);
		Console.WriteLine("# jsonRoundText:");
		Console.WriteLine(jsonRoundText);

		Assert.True(json.IsEquivalentTo(jsonRoundTripped));
	}
}