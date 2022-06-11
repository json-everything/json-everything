using System;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.More.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		JsonNode node = 1L;
		var copy = node.Copy();

		Console.WriteLine(node);
		Console.WriteLine(copy);

		Assert.AreNotSame(node, copy);
		node.IsEquivalentTo(copy);
	}
}
