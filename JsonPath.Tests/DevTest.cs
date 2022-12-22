using System;
using System.Text.Json.Nodes;
using Json.Path.Expressions;
using NUnit.Framework;

namespace Json.Path.Tests;

internal class DevTest
{
	[Test]
	public void Test()
	{
		var path = JsonPath.Parse("$[?4+5==9]");

		Console.WriteLine(path.Evaluate(null));
	}
}