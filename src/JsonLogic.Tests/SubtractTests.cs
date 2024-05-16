using Json.Logic.Rules;
using NUnit.Framework;
using TestHelpers;

namespace Json.Logic.Tests;

public class SubtractTests
{
	[Test]
	public void SubtractNumbersReturnsSum()
	{
		var rule = new SubtractRule(4, 5);

		var actual = rule.Apply();
		JsonAssert.AreEquivalent(-1, actual);
	}
}