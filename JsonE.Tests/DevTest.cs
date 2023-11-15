using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using Yaml2JsonNode;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Json.JsonE.Tests;

public class DevTest
{
	[Test]
	public void Check()
	{
		var yaml = Parse("\" \f\n\r\t\vabc \f\n\r\t\v\"");
		Console.WriteLine(Serialize(yaml));
	}

	public static YamlStream Parse(string yamlText)
	{
		using var reader = new StringReader(yamlText);
		var yamlStream = new YamlStream();
		yamlStream.Load(reader);
		return yamlStream;
	}

	public static string Serialize(YamlStream yaml)
	{
		var builder = new SerializerBuilder();
		var serializer = builder.Build();
		using var writer = new StringWriter();

		foreach (var document in yaml.Documents)
		{
			serializer.Serialize(writer, document.RootNode);
		}

		return writer.ToString();
	}
}