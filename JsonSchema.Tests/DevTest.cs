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
		var node = JsonNode.Parse("1238723762349702529873649378023892834969761287612402596");

		var value = node.GetValue<object>();

		Console.WriteLine(value.GetType());
	}
}