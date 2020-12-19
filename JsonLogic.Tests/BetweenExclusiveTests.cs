using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class BetweenExclusiveTests
	{
		[Test]
		public void BetweenValueInRangeReturnsTrue()
		{
			var rule = new BetweenExclusiveComponent(1, 2, 3);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void BetweenValueAtLowEndReturnsFalse()
		{
			var rule = new BetweenExclusiveComponent(1, 1, 3);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void BetweenValueUnderLowEndReturnsFalse()
		{
			var rule = new BetweenExclusiveComponent(1, 0, 3);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void BetweenValueAtHighEndReturnsFalse()
		{
			var rule = new BetweenExclusiveComponent(1, 3, 3);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void BetweenValueOverHighEndReturnsFalse()
		{
			var rule = new BetweenExclusiveComponent(1, 4, 3);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void BetweenLowEndNotNumberThrowsError()
		{
			var rule = new BetweenExclusiveComponent(false, 4, 3);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void BetweenValueNotNumberThrowsError()
		{
			var rule = new BetweenExclusiveComponent(1, false, 3);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void BetweenHighEndNotNumberThrowsError()
		{
			var rule = new BetweenExclusiveComponent(1, 2, false);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}
