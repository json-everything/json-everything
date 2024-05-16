using Json.Logic.Rules;
using NUnit.Framework;
using TestHelpers;

namespace Json.Logic.Tests;

public class AddTests
{
	[Test]
	public void AddNumbersReturnsSum()
	{
		var rule = new AddRule(4, 5);

		var actual = rule.Apply();
		JsonAssert.AreEquivalent(9, actual);
	}

	[Test]
	public void AddSingleNumberDoesNothing()
	{
		var rule = new AddRule(3.14);

		var actual = rule.Apply();
		JsonAssert.AreEquivalent(3.14, actual);
	}

	[Test]
	public void AddSingleStringWithNumberCasts()
	{
		var rule = new AddRule("3.14");

		var actual = rule.Apply();
		JsonAssert.AreEquivalent(3.14, actual);
	}

	[Test]
	public void AddSingleTrueThrowsError()
	{
		var rule = new AddRule(true);

		JsonAssert.AreEquivalent(1, rule.Apply());
	}

	[Test]
	public void AddSingleFalseThrowsError()
	{
		var rule = new AddRule(false);

		JsonAssert.AreEquivalent(0, rule.Apply());
	}

	[Test]
	public void AddSingleNullReturns0()
	{
		var rule = new AddRule(LiteralRule.Null);

		JsonAssert.AreEquivalent(0, rule.Apply());
	}
}