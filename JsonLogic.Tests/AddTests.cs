using System.Text.Json;
using Json.Logic.Components;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class AddTests
	{
		[Test]
		public void AddNumbersReturnsSum()
		{
			var rule = new AddComponent(new LiteralComponent(4), new LiteralComponent(5));

			var actual = rule.Apply();
			JsonAssert.AreEquivalent(9, actual);
		}

		[Test]
		public void AddNonNumberThrowsError()
		{
			var rule = new AddComponent(new LiteralComponent("test"), new LiteralComponent(5));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void AddSingleNumberDoesNothing()
		{
			var rule = new AddComponent(new LiteralComponent(3.14));

			var actual = rule.Apply();
			JsonAssert.AreEquivalent(3.14, actual);
		}

		[Test]
		public void AddSingleStringWithNumberCasts()
		{
			var rule = new AddComponent(new LiteralComponent("3.14"));

			var actual = rule.Apply();
			JsonAssert.AreEquivalent(3.14, actual);
		}

		[Test]
		public void AddSingleStringWithJunkThrowsError()
		{
			var rule = new AddComponent(new LiteralComponent("foo"));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void AddSingleArrayThrowsError()
		{
			var rule = new AddComponent(new LiteralComponent(new[] {false.AsJsonElement(), 5.AsJsonElement()}.AsJsonElement()));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void AddSingleObjectThrowsError()
		{
			var rule = new AddComponent(new LiteralComponent(JsonDocument.Parse(JsonSerializer.Serialize(new {foo = 5})).RootElement));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void AddSingleTrueThrowsError()
		{
			var rule = new AddComponent(new LiteralComponent(true));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void AddSingleFalseThrowsError()
		{
			var rule = new AddComponent(new LiteralComponent(false));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}

		[Test]
		public void AddSingleNullThrowsError()
		{
			var rule = new AddComponent(new LiteralComponent(JsonDocument.Parse("null").RootElement));

			Assert.Throws<JsonLogicException>(() => rule.Apply());
		}
	}
}