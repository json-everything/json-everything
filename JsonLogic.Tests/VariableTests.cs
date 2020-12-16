using Json.Logic.Components;
using Json.More;
using Json.Path;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class VariableTests
	{
		[Test]
		public void VariableWithValidPathAndNoDefaultFetchesData()
		{
			var rule = new VariableComponent(JsonPath.Parse("$.foo"));
			var data = new {foo = 5, bar = 10}.ToJsonDocument().RootElement;

			JsonAssert.AreEquivalent(5, rule.Apply(data));
		}

		[Test]
		public void VariableWithInvalidPathAndNoDefaultThrowsError()
		{
			var rule = new VariableComponent(JsonPath.Parse("$.baz"));
			var data = new {foo = 5, bar = 10}.ToJsonDocument().RootElement;

			Assert.Throws<JsonLogicException>(() => rule.Apply(data));
		}

		[Test]
		public void VariableWithValidPathAndDefaultFetchesData()
		{
			var rule = new VariableComponent(JsonPath.Parse("$.foo"), new LiteralComponent(11));
			var data = new {foo = 5, bar = 10}.ToJsonDocument().RootElement;

			JsonAssert.AreEquivalent(5, rule.Apply(data));
		}

		[Test]
		public void VariableWithInvalidPathAndDefaultReturnsDefault()
		{
			var rule = new VariableComponent(JsonPath.Parse("$.baz"), new LiteralComponent(11));
			var data = new {foo = 5, bar = 10}.ToJsonDocument().RootElement;

			JsonAssert.AreEquivalent(11, rule.Apply(data));
		}
	}
}