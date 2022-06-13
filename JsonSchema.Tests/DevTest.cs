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
		var json = JsonNode.Parse("[{\"foo\":1, \"bar\":2, \"foo\":3}]") as JsonArray;

		var item = json[0];
	}
}