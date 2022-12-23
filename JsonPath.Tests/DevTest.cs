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
		ValueExpressionParser.TryParse("4*(5+6)", ref i, out var expr);

		Console.WriteLine(expr.Evaluate(null, null));
	}
}