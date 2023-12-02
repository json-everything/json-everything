using System;
using System.Text.Json;
using System.Text.Json.Nodes;
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