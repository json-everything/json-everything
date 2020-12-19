using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class DivideTests
	{
		[Test]
		public void DivideNumbersReturnsSum()
		{
			var rule = new DivideComponent(4, 5);

			var actual = rule.Apply();
			JsonAssert.AreEquivalent(.8, actual);
		}

		[Test]
		public void DivideNonNumberThrowsError()
		{
			var rule = new DivideComponent("test", 5);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void DivideByZeroThrowsError()
		{
			var rule = new DivideComponent(4, 0);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}