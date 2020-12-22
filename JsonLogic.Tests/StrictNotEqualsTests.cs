using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class StrictNotEqualsTests
	{
		[Test]
		public void NotEqualReturnsTrue()
		{
			var rule = new StrictNotEqualsRule(1, 2);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void EqualsReturnsFalse()
		{
			var rule = new StrictNotEqualsRule(1, 1);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void LooseNotEqualsReturnsTrue()
		{
			var rule = new StrictNotEqualsRule(1, "1");

			JsonAssert.IsTrue(rule.Apply());
		}
	}
}