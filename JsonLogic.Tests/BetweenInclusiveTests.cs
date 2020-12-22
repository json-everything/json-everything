using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class BetweenInclusiveTests
	{
		[Test]
		public void BetweenValueInRangeReturnsTrue()
		{
			var rule = new BetweenInclusiveRule(1, 2, 3);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void BetweenValueAtLowEndReturnsTrue()
		{
			var rule = new BetweenInclusiveRule(1, 1, 3);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void BetweenValueUnderLowEndReturnsFalse()
		{
			var rule = new BetweenInclusiveRule(1, 0, 3);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void BetweenValueAtHighEndReturnsTrue()
		{
			var rule = new BetweenInclusiveRule(1, 3, 3);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void BetweenValueOverHighEndReturnsFalse()
		{
			var rule = new BetweenInclusiveRule(1, 4, 3);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void BetweenLowEndNotNumberThrowsError()
		{
			var rule = new BetweenInclusiveRule(false, 4, 3);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void BetweenValueNotNumberThrowsError()
		{
			var rule = new BetweenInclusiveRule(1, false, 3);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void BetweenHighEndNotNumberThrowsError()
		{
			var rule = new BetweenInclusiveRule(1, 2, false);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}
