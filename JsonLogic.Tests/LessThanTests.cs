using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests;

public class LessThanTests
{
	[Test]
	public void LessThanTwoNumbersReturnsTrue()
	{
		var rule = new LessThanRule(1, 2);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void EqualTwoNumbersReturnsFalse()
	{
		var rule = new LessThanRule(1, 1);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void LessThanTwoNumbersReturnsFalse()
	{
		var rule = new LessThanRule(3, 2);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void LessThanStringThrowsError()
	{
		var rule = new LessThanRule("foo", 2);

		Assert.Throws<JsonLogicException>(() => rule.Apply());
	}

	[Test]
	public void LessThanBooleanReturnsTrue()
	{
		var rule = new LessThanRule(false, 2);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void LessThanArrayThrowsError()
	{
		var rule = new LessThanRule(new JsonArray(), 2);

		Assert.Throws<JsonLogicException>(() => rule.Apply());
	}

	[Test]
	public void LessThanObjectThrowsError()
	{
		var rule = new LessThanRule(new JsonObject(), 2);

		Assert.Throws<JsonLogicException>(() => rule.Apply());
	}

	[Test]
	public void LessThanNullCastsNullToZero()
	{
		var rule = new LessThanRule(LiteralRule.Null, 2);

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void LessThanNumberAndStringNumberReturnsTrue()
	{
		var rule = new LessThanRule(1, "2");

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void LessThanStringNumberAndNumberReturnsTrue()
	{
		var rule = new LessThanRule("1", 2);

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void LessThanTwoStringNumbersReturnsTrue()
	{
		var rule = new LessThanRule("1", "2");

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void BetweenValueInRangeReturnsTrue()
	{
		var rule = new LessThanRule(1, 2, 3);

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void BetweenValueAtLowEndReturnsFalse()
	{
		var rule = new LessThanRule(1, 1, 3);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void BetweenValueUnderLowEndReturnsFalse()
	{
		var rule = new LessThanRule(1, 0, 3);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void BetweenValueAtHighEndReturnsFalse()
	{
		var rule = new LessThanRule(1, 3, 3);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void BetweenValueOverHighEndReturnsFalse()
	{
		var rule = new LessThanRule(1, 4, 3);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void BetweenLowEndNotNumberReturnsFalse()
	{
		var rule = new LessThanRule(false, 4, 3);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void BetweenValueNotNumberReturnsFalse()
	{
		var rule = new LessThanRule(1, false, 3);

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void BetweenHighEndNotNumberReturnsFalse()
	{
		var rule = new LessThanRule(1, 2, false);

		JsonAssert.IsFalse(rule.Apply());
	}
	
	[Test]
	public void BetweenStringNumbersInRangeReturnsTrue()
	{
		var rule = new LessThanRule("1", "2", "3");

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void BetweenLowEndStringNumberReturnsTrue()
	{
		var rule = new LessThanRule("1", 2, 3);

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void BetweenValueStringNumberReturnsTrue()
	{
		var rule = new LessThanRule(1, "2", 3);

		JsonAssert.IsTrue(rule.Apply());
	}
	
	[Test]
	public void BetweenHighEndStringNumberReturnsTrue()
	{
		var rule = new LessThanRule(1, 2, "3");

		JsonAssert.IsTrue(rule.Apply());
	}
}