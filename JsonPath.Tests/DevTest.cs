using System;
using Json.Path.Expressions;
using NUnit.Framework;

namespace Json.Path.Tests;

internal class DevTest
{
	[Test]
	public void Test()
	{
		var i = 0;
		BooleanResultExpressionParser.TryParse(" 4 == @.foo && 5 == ( 5 ) ].foo", ref i, out var expr);

		Console.WriteLine(expr.Evaluate(null, null));
	}
}