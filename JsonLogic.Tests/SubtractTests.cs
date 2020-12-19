using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class SubtractTests
	{
		[Test]
		public void SubtractNumbersReturnsSum()
		{
			var rule = new SubtractComponent(4, 5);

			var actual = rule.Apply();
			JsonAssert.AreEquivalent(-1, actual);
		}

		[Test]
		public void SubtractNonNumberThrowsError()
		{
			var rule = new SubtractComponent("test", 5);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}