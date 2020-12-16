using System.Text.Json;
using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class NotTests
	{
		[Test]
		public void EmptyArrayIsTrue()
		{
			var rule = new NotComponent(new LiteralComponent(JsonDocument.Parse("[]").RootElement));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void NonEmptyArrayIsFalse()
		{
			var rule = new NotComponent(new LiteralComponent(JsonDocument.Parse("[1]").RootElement));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void EmptyStringIsTrue()
		{
			var rule = new NotComponent(new LiteralComponent(""));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void NonEmptyStringIsFalse()
		{
			var rule = new NotComponent(new LiteralComponent("foo"));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void ZeroIsTrue()
		{
			var rule = new NotComponent(new LiteralComponent(0));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void NonZeroIsFalse()
		{
			var rule = new NotComponent(new LiteralComponent(1));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void FalseIsTrue()
		{
			var rule = new NotComponent(new LiteralComponent(false));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void TrueIsFalse()
		{
			var rule = new NotComponent(new LiteralComponent(true));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void NullIsTrue()
		{
			var rule = new NotComponent(new LiteralComponent(JsonDocument.Parse("null").RootElement));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void ObjectThrowsError()
		{
			var rule = new NotComponent(new LiteralComponent(JsonDocument.Parse("{}").RootElement));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}