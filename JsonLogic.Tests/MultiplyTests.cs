using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class MultiplyTests
	{
		[Test]
		public void MultiplyNumbersReturnsSum()
		{
			var rule = new MultiplyComponent(4, 5);

			var actual = rule.Apply();
			JsonAssert.AreEquivalent(20, actual);
		}

		[Test]
		public void MultiplyNonNumberThrowsError()
		{
			var rule = new MultiplyComponent("test", 5);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}