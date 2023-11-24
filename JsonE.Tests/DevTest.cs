using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.JsonE.Tests;

public class DevTest
{
	[Test]
	public void Check()
	{
		var value = JsonNode.Parse("\"\\uE1FF\"");

		Console.WriteLine(JsonSerializer.Serialize(value, new JsonSerializerOptions
		{
			//Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		}));
	}
}