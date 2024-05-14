using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests;

public class MoreThanEqualTests
{
	[Test]
	public void MoreThanEqualTwoNumbersReturnsTrue()
	{
		var rule = new MoreThanEqualRule(2, 1);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void EqualTwoNumbersReturnsTrue()
	{
		var rule = new MoreThanEqualRule(1, 1);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void MoreThanEqualTwoNumbersReturnsFalse()
	{
		var rule = new MoreThanEqualRule(2, 3);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void MoreThanEqualBooleanThrowsError()
	{
		var rule = new MoreThanEqualRule(false, 2);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void MoreThanEqualNullCastsNullToZero()
	{
		var rule = new MoreThanEqualRule(LiteralRule.Null, 2);

		JsonAssert.IsFalse(rule.Apply());
	}
}