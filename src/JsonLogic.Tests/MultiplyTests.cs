using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests;

public class MultiplyTests
{
	[Test]
	public void MultiplyNumbersReturnsSum()
	{
		var rule = new MultiplyRule(4, 5);

		var actual = rule.Apply();
		JsonAssert.AreEquivalent(20, actual);
	}
}