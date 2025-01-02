using Json.Logic.Rules;
using NUnit.Framework;
using TestHelpers;

namespace Json.Logic.Tests;

public class AddTests
{
	[Test]
	public void AddNumbersReturnsSum()
	{
		var rule = JsonLogic.Add(4, 5);

		var actual = rule.Apply();
		JsonAssert.AreEquivalent(9, actual);
	}

	[Test]
	public void AddSingleNumberDoesNothing()
	{
		var rule = JsonLogic.Add(3.14);

		var actual = rule.Apply();
		JsonAssert.AreEquivalent(3.14, actual);
	}

	[Test]
	public void AddSingleStringWithNumberCasts()
	{
		var rule = JsonLogic.Add("3.14");

		var actual = rule.Apply();
		JsonAssert.AreEquivalent(3.14, actual);
	}

	[Test]
	public void AddSingleTrueThrowsError()
	{
		var rule = JsonLogic.Add(true);

		JsonAssert.AreEquivalent(1, rule.Apply());
	}

	[Test]
	public void AddSingleFalseThrowsError()
	{
		var rule = JsonLogic.Add(false);

		JsonAssert.AreEquivalent(0, rule.Apply());
	}

	[Test]
	public void AddSingleNullReturns0()
	{
		var rule = JsonLogic.Add(LiteralRule.Null);

		JsonAssert.AreEquivalent(0, rule.Apply());
	}
}