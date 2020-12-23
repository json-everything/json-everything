using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class DivideTests
	{
		[Test]
		public void DivideNumbersReturnsSum()
		{
			var rule = new DivideRule(4, 5);

			var actual = rule.Apply();
			JsonAssert.AreEquivalent(.8, actual);
		}

		[Test]
		public void DivideNonNumberThrowsError()
		{
			var rule = new DivideRule("test", 5);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void DivideByZeroThrowsError()
		{
			var rule = new DivideRule(4, 0);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}