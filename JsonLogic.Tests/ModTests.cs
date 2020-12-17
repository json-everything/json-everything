using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class ModTests
	{
		[Test]
		public void ModNumbersReturnsSum()
		{
			var rule = new ModComponent(new LiteralComponent(4), new LiteralComponent(5));

			var actual = rule.Apply();
			JsonAssert.AreEquivalent(4, actual);
		}

		[Test]
		public void ModNonNumberThrowsError()
		{
			var rule = new ModComponent(new LiteralComponent("test"), new LiteralComponent(5));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void ModByZeroThrowsError()
		{
			var rule = new ModComponent(new LiteralComponent(4), new LiteralComponent(0));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}