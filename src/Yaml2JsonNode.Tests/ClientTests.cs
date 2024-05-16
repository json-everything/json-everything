using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;
using TestHelpers;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

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
		var jsonRoundText = jsonRoundTripped!.ToJsonString();

		Console.WriteLine("# jsonText:");
		Console.WriteLine(jsonText);
		Console.WriteLine();
		Console.WriteLine("# yamlText:");
		Console.WriteLine(yamlText);
		Console.WriteLine("# jsonRoundText:");
		Console.WriteLine(jsonRoundText);

		JsonAssert.AreEquivalent(json, jsonRoundTripped);
	}

	[Test]
	public void Issue476_YamlNumberAsString()
	{
		var yamlValue = new YamlScalarNode("123");
		var yamlKey = new YamlScalarNode("a");

		var yamlObject = new YamlMappingNode(new Dictionary<YamlNode, YamlNode>
		{
			[yamlKey] = yamlValue
		});

		var builder = new SerializerBuilder();
		var serializer = builder.Build();
		using var writer = new StringWriter();
		serializer.Serialize(writer, yamlObject);
		var text = writer.ToString();

		Console.WriteLine(text);
	}

	[Test]
	public void Issue478_DecimalFormatting_Comma()
	{
		var culture = CultureInfo.CurrentCulture;

		try
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");

			var yamlText = @"data: 4,789";

			var expected = new JsonObject { ["data"] = 4789 };

			var actual = YamlSerializer.Parse(yamlText).Single().ToJsonNode();

			JsonAssert.AreEquivalent(expected, actual);
		}
		finally
		{
			CultureInfo.CurrentCulture = culture;
		}
	}

	[Test]
	public void Issue478_DecimalFormatting_Dot()
	{
		var culture = CultureInfo.CurrentCulture;

		try
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");

			var yamlText = @"data: 4.789";

			var expected = new JsonObject { ["data"] = 4.789 };

			var actual = YamlSerializer.Parse(yamlText).Single().ToJsonNode();

			JsonAssert.AreEquivalent(expected, actual);
		}
		finally
		{
			CultureInfo.CurrentCulture = culture;
		}
	}

	[Test]
	public void Issue485_YamlNullNotConvertingRight()
	{
		var yaml = @"nullVal: null
altNullVal: ~
implicitNullVal: 
dblQuotedVal: ""null""
quotedVal: 'null'
rawVal: |
  null
rawValTrim: |-
  null";
		var yamlStream = new YamlStream();
		yamlStream.Load(new StringReader(yaml));
		var yamlDoc = yamlStream.Documents[0];

		JsonNode expected = new JsonObject
		{
			["nullVal"] = null,
			["altNullVal"] = null,
			["implicitNullVal"] = null,
			["dblQuotedVal"] = "null",
			["quotedVal"] = "null",
			["rawVal"] = "null\n",
			["rawValTrim"] = "null"
		};

		var actual = yamlDoc.ToJsonNode();

		Console.WriteLine(actual.AsJsonString(new JsonSerializerOptions { WriteIndented = true }));

		JsonAssert.AreEquivalent(expected, actual);
	}
}