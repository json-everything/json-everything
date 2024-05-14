using System;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.JsonE.Tests;

public class CustomFunctionTests
{
	[Test]
	public void ModFunction()
	{
		var context = new JsonObject
		{
			["mod"] = JsonFunction.Create((parameters, context) =>
			{
				var a = parameters[0]?.AsValue().GetNumber();
				var b = parameters[1]?.AsValue().GetNumber();

				return a % b;
			})
		};

		var template = new JsonObject
		{
			["$eval"] = "mod(10, 4)"
		};


		var result = JsonE.Evaluate(template, context);

		Console.WriteLine(result);
	}
}