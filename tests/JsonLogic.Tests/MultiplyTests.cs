using NUnit.Framework;
using TestHelpers;

namespace Json.Logic.Tests;

public class MultiplyTests
{
	[Test]
	public void MultiplyNumbersReturnsSum()
	{
		var rule = JsonLogic.Multiply(4, 5);

		var actual = rule.Apply();
		JsonAssert.AreEquivalent(20, actual);
	}
}