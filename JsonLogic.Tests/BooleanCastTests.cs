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
			var rule = new BooleanCastComponent(new LiteralComponent(JsonDocument.Parse("[]").RootElement));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void NonEmptyArrayIsTrue()
		{
			var rule = new BooleanCastComponent(new LiteralComponent(JsonDocument.Parse("[1]").RootElement));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void EmptyStringIsFalse()
		{
			var rule = new BooleanCastComponent(new LiteralComponent(""));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void NonEmptyStringIsTrue()
		{
			var rule = new BooleanCastComponent(new LiteralComponent("foo"));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void ZeroIsFalse()
		{
			var rule = new BooleanCastComponent(new LiteralComponent(0));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void NonZeroIsTrue()
		{
			var rule = new BooleanCastComponent(new LiteralComponent(1));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void FalseIsFalse()
		{
			var rule = new BooleanCastComponent(new LiteralComponent(false));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void TrueIsTrue()
		{
			var rule = new BooleanCastComponent(new LiteralComponent(true));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void NullIsFalse()
		{
			var rule = new BooleanCastComponent(new LiteralComponent(JsonDocument.Parse("null").RootElement));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void ObjectThrowsError()
		{
			var rule = new BooleanCastComponent(new LiteralComponent(JsonDocument.Parse("{}").RootElement));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}
