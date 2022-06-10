using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.More.Tests;

public class DevTest
{
	[Test]
	[Ignore("for development purposes")]
	public void Test()
	{
		JsonNode node = 1L;

		Console.WriteLine(node.GetValue<decimal>());
		Console.WriteLine(node.GetValue<int>());
		Console.WriteLine(node.GetValue<float>());
	}
}
