using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class StrictEqualsTests
	{
		[Test]
		public void NotEqualReturnsFalse()
		{
			var rule = new StrictEqualsComponent(1, 2);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void EqualsReturnsTrue()
		{
			var rule = new StrictEqualsComponent(1, 1);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void LooseEqualsReturnsFalse()
		{
			var rule = new StrictEqualsComponent(1, "1");

			JsonAssert.IsFalse(rule.Apply());
		}
	}
}