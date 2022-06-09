using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.More.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		var node = JsonSerializer.Deserialize<JsonValue>("5");

		Console.WriteLine(node.GetValue<decimal>());
		Console.WriteLine(node.GetValue<int>());
		Console.WriteLine(node.GetValue<float>());
	}
}