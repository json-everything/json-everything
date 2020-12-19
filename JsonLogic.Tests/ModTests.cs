using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class ModTests
	{
		[Test]
		public void ModNumbersReturnsSum()
		{
			var rule = new ModComponent(4, 5);

			var actual = rule.Apply();
			JsonAssert.AreEquivalent(4, actual);
		}

		[Test]
		public void ModNonNumberThrowsError()
		{
			var rule = new ModComponent("test", 5);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void ModByZeroThrowsError()
		{
			var rule = new ModComponent(4, 0);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}