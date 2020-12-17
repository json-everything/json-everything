using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class SubtractTests
	{
		[Test]
		public void SubtractNumbersReturnsSum()
		{
			var rule = new SubtractComponent(new LiteralComponent(4), new LiteralComponent(5));

			var actual = rule.Apply();
			JsonAssert.AreEquivalent(-1, actual);
		}

		[Test]
		public void SubtractNonNumberThrowsError()
		{
			var rule = new SubtractComponent(new LiteralComponent("test"), new LiteralComponent(5));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}