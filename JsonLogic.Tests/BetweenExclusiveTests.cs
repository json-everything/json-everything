using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class BetweenExclusiveTests
	{
		[Test]
		public void BetweenValueInRangeReturnsTrue()
		{
			var rule = new BetweenExclusiveComponent(new LiteralComponent(1), new LiteralComponent(2), new LiteralComponent(3));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void BetweenValueAtLowEndReturnsFalse()
		{
			var rule = new BetweenExclusiveComponent(new LiteralComponent(1), new LiteralComponent(1), new LiteralComponent(3));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void BetweenValueUnderLowEndReturnsFalse()
		{
			var rule = new BetweenExclusiveComponent(new LiteralComponent(1), new LiteralComponent(0), new LiteralComponent(3));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void BetweenValueAtHighEndReturnsFalse()
		{
			var rule = new BetweenExclusiveComponent(new LiteralComponent(1), new LiteralComponent(3), new LiteralComponent(3));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void BetweenValueOverHighEndReturnsFalse()
		{
			var rule = new BetweenExclusiveComponent(new LiteralComponent(1), new LiteralComponent(4), new LiteralComponent(3));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void BetweenLowEndNotNumberThrowsError()
		{
			var rule = new BetweenExclusiveComponent(new LiteralComponent(false), new LiteralComponent(4), new LiteralComponent(3));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void BetweenValueNotNumberThrowsError()
		{
			var rule = new BetweenExclusiveComponent(new LiteralComponent(1), new LiteralComponent(false), new LiteralComponent(3));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void BetweenHighEndNotNumberThrowsError()
		{
			var rule = new BetweenExclusiveComponent(new LiteralComponent(1), new LiteralComponent(2), new LiteralComponent(false));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}
