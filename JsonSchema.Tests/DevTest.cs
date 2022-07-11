using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		var json = new JsonObject();

		Console.WriteLine(JsonSerializer.Serialize(json));
	}
}