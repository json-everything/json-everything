using System.Text.Json;
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

			Assert.AreEqual(JsonValueKind.False, rule.Apply().ValueKind);
		}

		[Test]
		public void EqualsReturnsTrue()
		{
			var rule = new StrictEqualsComponent(new LiteralComponent(1), new LiteralComponent(1));

			Assert.AreEqual(JsonValueKind.True, rule.Apply().ValueKind);
		}
	}
}