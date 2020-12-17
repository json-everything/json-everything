using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class StrictEqualsTests
	{
		[Test]
		public void NotEqualReturnsFalse()
		{
			var rule = new StrictEqualsComponent(new LiteralComponent(1), new LiteralComponent(2));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void EqualsReturnsTrue()
		{
			var rule = new StrictEqualsComponent(new LiteralComponent(1), new LiteralComponent(1));

			JsonAssert.IsTrue(rule.Apply());
		}
	}
}