using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class MoreThanTests
	{
		[Test]
		public void MoreThanReturnsTrue()
		{
			var rule = new MoreThanComponent(new LiteralComponent(5), new LiteralComponent(4));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void MoreThanReturnsFalse()
		{
			var rule = new MoreThanComponent(new LiteralComponent(4), new LiteralComponent(5));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void MoreThanNonNumberThrowsError()
		{
			var rule = new MoreThanComponent(new LiteralComponent(false), new LiteralComponent(5));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}