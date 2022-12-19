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
		LiteralExpressionNode four = (JsonNode)4;
		LiteralExpressionNode five = (JsonNode)5;
		PathExpressionNode path = JsonPath.Parse("@['foo']");

		var expression = !(four + path != four * five | four > five);

		Console.WriteLine(expression.Evaluate(null, null));
	}
}