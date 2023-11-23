using System;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions.Functions;
using Json.More;
using NUnit.Framework;

namespace Json.JsonE.Tests;

public class DevTest
{
	[Test]
	public void Check()
	{
		var context = new JsonObject
		{
			["test"] = JsonFunction.Create((arguments, context) => true)
		};

		var result = JsonE.Evaluate(new JsonObject { ["$eval"] = "test(1)" }, context);

		Console.WriteLine(result.AsJsonString());
	}
}