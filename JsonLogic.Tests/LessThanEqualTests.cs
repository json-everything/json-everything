using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests;

public class LessThanEqualTests
{
	[Test]
	public void LessThanTwoNumbersReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, 2);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void EqualTwoNumbersReturnsTrue()
	{
		var rule = new LessThanEqualRule(2, 2);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void LessThanEqualTwoNumbersReturnsFalse()
	{
		var rule = new LessThanEqualRule(3, 2);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void LessThanEqualStringThrowsError()
	{
		var rule = new LessThanEqualRule("foo", 2);

		Assert.Throws<JsonLogicException>(() => rule.Apply());
	}

	[Test]
	public void LessThanEqualBooleanThrowsError()
	{
		var rule = new LessThanEqualRule(false, 2);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void LessThanEqualArrayThrowsError()
	{
		var rule = new LessThanEqualRule(new JsonArray(), 2);

		Assert.Throws<JsonLogicException>(() => rule.Apply());
	}

	[Test]
	public void LessThanEqualObjectThrowsError()
	{
		var rule = new LessThanEqualRule(new JsonObject(), 2);

		Assert.Throws<JsonLogicException>(() => rule.Apply());
	}

	[Test]
	public void LessThanEqualNullCastsNullToZero()
	{
		var rule = new LessThanEqualRule(LiteralRule.Null, 2);

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void LessThanEqualNumberAndStringNumberReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, "2");

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void LessThanEqualStringNumberAndNumberReturnsTrue()
	{
		var rule = new LessThanEqualRule("1", 1);

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void LessThanEqualTwoStringNumbersReturnsTrue()
	{
		var rule = new LessThanEqualRule("1", "2");

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void BetweenValueInRangeReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, 2, 3);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void BetweenValueAtLowEndReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, 1, 3);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void BetweenValueUnderLowEndReturnsFalse()
	{
		var rule = new LessThanEqualRule(1, 0, 3);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void BetweenValueAtHighEndReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, 3, 3);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void BetweenValueOverHighEndReturnsFalse()
	{
		var rule = new LessThanEqualRule(1, 4, 3);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void BetweenLowEndNotNumberReturnsFalse()
	{
		var rule = new LessThanEqualRule(false, 4, 3);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void BetweenValueNotNumberReturnsFalse()
	{
		var rule = new LessThanEqualRule(1, false, 3);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void BetweenHighEndNotNumberReturnsFalse()
	{
		var rule = new LessThanEqualRule(1, 2, false);

		JsonAssert.IsFalse(rule.Apply());
	}
	
	[Test]
	public void BetweenLowEndStringNumberReturnsTrue()
	{
		var rule = new LessThanEqualRule("1", 1, 3);

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void BetweenValueStringNumberReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, "2", 3);

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void BetweenHighEndStringNumberReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, 2, "3");

		JsonAssert.IsTrue(rule.Apply());
	}
}