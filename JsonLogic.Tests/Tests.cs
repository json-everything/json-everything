using System.Text.Json;
using Json.Logic.Components;
using Json.More;
using Json.Path;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class Tests
	{
		[Test]
		public void BasicEquals_False()
		{
			var rule = new StrictEqualsComponent(new LiteralComponent(1), new LiteralComponent(2));

			Assert.AreEqual(JsonValueKind.False, rule.Apply("null".AsJsonElement()).ValueKind);
		}

		[Test]
		public void BasicEquals_True()
		{
			var rule = new StrictEqualsComponent(new LiteralComponent(1), new LiteralComponent(1));

			Assert.AreEqual(JsonValueKind.True, rule.Apply("null".AsJsonElement()).ValueKind);
		}

		[Test]
		public void BasicVariable_NoDefault_FetchesData()
		{
			var rule = new VariableComponent(JsonPath.Parse("$.foo"));
			var data = new {foo = 5, bar = 10}.ToJsonDocument().RootElement;

			Assert.IsTrue(5.AsJsonElement().IsEquivalentTo(rule.Apply(data)));
		}

		[Test]
		public void EqualsVariable_True()
		{
			var rule = new StrictEqualsComponent(new VariableComponent(JsonPath.Parse("$.foo")), new LiteralComponent(5));
			var data = new {foo = 5, bar = 10}.ToJsonDocument().RootElement;

			Assert.AreEqual(JsonValueKind.True, rule.Apply(data).ValueKind);
		}

		[Test]
		public void EqualsVariable_False_ValueNotEqual()
		{
			var rule = new StrictEqualsComponent(new VariableComponent(JsonPath.Parse("$.foo")), new LiteralComponent(5));
			var data = new {foo = 15, bar = 10}.ToJsonDocument().RootElement;

			Assert.AreEqual(JsonValueKind.False, rule.Apply(data).ValueKind);
		}

		[Test]
		public void EqualsVariable_True_PathNotFound_DefaultEqual()
		{
			var rule = new StrictEqualsComponent(new VariableComponent(JsonPath.Parse("$.foo"), new LiteralComponent(5.AsJsonElement())), new LiteralComponent(5));
			var data = new {food = 5, bar = 10}.ToJsonDocument().RootElement;

			Assert.AreEqual(JsonValueKind.False, rule.Apply(data).ValueKind);
		}

		[Test]
		public void EqualsVariable_False_PathNotFound_DefaultNotEqual()
		{
			var rule = new StrictEqualsComponent(new VariableComponent(JsonPath.Parse("$.foo"), new LiteralComponent(15.AsJsonElement())), new LiteralComponent(5));
			var data = new {food = 5, bar = 10}.ToJsonDocument().RootElement;

			Assert.AreEqual(JsonValueKind.False, rule.Apply(data).ValueKind);
		}
	}
}