using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class SubtractTests
	{
		[Test]
		public void SubtractNumbersReturnsSum()
		{
			var rule = new SubtractRule(4, 5);

			var actual = rule.Apply();
			JsonAssert.AreEquivalent(-1, actual);
		}

		[Test]
		public void SubtractNonNumberThrowsError()
		{
			var rule = new SubtractRule("test", 5);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}