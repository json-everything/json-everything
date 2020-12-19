using System.Text.Json;
using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class BooleanCastTests
	{
		[Test]
		public void EmptyArrayIsFalse()
		{
			var rule = new BooleanCastComponent(JsonDocument.Parse("[]").RootElement);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void NonEmptyArrayIsTrue()
		{
			var rule = new BooleanCastComponent(JsonDocument.Parse("[1]").RootElement);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void EmptyStringIsFalse()
		{
			var rule = new BooleanCastComponent("");

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void NonEmptyStringIsTrue()
		{
			var rule = new BooleanCastComponent("foo");

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void ZeroIsFalse()
		{
			var rule = new BooleanCastComponent(0);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void NonZeroIsTrue()
		{
			var rule = new BooleanCastComponent(1);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void FalseIsFalse()
		{
			var rule = new BooleanCastComponent(false);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void TrueIsTrue()
		{
			var rule = new BooleanCastComponent(true);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void NullIsFalse()
		{
			var rule = new BooleanCastComponent(JsonDocument.Parse("null").RootElement);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void ObjectThrowsError()
		{
			var rule = new BooleanCastComponent(JsonDocument.Parse("{}").RootElement);

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}
