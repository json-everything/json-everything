using System.Text.Json;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class NotTests
	{
		[Test]
		public void EmptyArrayIsTrue()
		{
			var rule = new NotRule(JsonDocument.Parse("[]").RootElement);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void NonEmptyArrayIsFalse()
		{
			var rule = new NotRule(JsonDocument.Parse("[1]").RootElement);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void EmptyStringIsTrue()
		{
			var rule = new NotRule("");

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void NonEmptyStringIsFalse()
		{
			var rule = new NotRule("foo");

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void ZeroIsTrue()
		{
			var rule = new NotRule(0);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void NonZeroIsFalse()
		{
			var rule = new NotRule(1);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void FalseIsTrue()
		{
			var rule = new NotRule(false);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void TrueIsFalse()
		{
			var rule = new NotRule(true);

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void NullIsTrue()
		{
			var rule = new NotRule(JsonDocument.Parse("null").RootElement);

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void EmptyObjectIsTrue()
		{
			var rule = new NotRule(JsonDocument.Parse("{}").RootElement);

            JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void NonEmptyObjectIsFalse()
		{
			var rule = new NotRule(JsonDocument.Parse("{\"foo\":5}").RootElement);

            JsonAssert.IsFalse(rule.Apply());
		}
	}
}