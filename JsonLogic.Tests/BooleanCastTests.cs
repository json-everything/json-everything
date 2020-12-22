using System.Text.Json;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class BooleanCastTests
	{
		[Test]
		public void EmptyArrayIsFalse()
		{
			var rule = new BooleanCastRule(JsonDocument.Parse("[]").RootElement);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void NonEmptyArrayIsTrue()
		{
			var rule = new BooleanCastRule(JsonDocument.Parse("[1]").RootElement);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void EmptyStringIsFalse()
		{
			var rule = new BooleanCastRule("");

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void NonEmptyStringIsTrue()
		{
			var rule = new BooleanCastRule("foo");

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void ZeroIsFalse()
		{
			var rule = new BooleanCastRule(0);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void NonZeroIsTrue()
		{
			var rule = new BooleanCastRule(1);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void FalseIsFalse()
		{
			var rule = new BooleanCastRule(false);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void TrueIsTrue()
		{
			var rule = new BooleanCastRule(true);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void NullIsFalse()
		{
			var rule = new BooleanCastRule(JsonDocument.Parse("null").RootElement);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void ObjectThrowsError()
		{
			var rule = new BooleanCastRule(JsonDocument.Parse("{}").RootElement);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}
