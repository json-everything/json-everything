using System.Text.Json;
using Json.Logic.Components;
using Json.Path;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class SomeTests
	{
		[Test]
		public void SomeMatchCondition()
		{
			var rule = new SomeComponent(new LiteralComponent(JsonDocument.Parse("[1,2,3]").RootElement),
				new StrictEqualsComponent(new VariableComponent(JsonPath.Parse("$")), new LiteralComponent(2)));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void SomeDoNotMatchCondition()
		{
			var rule = new SomeComponent(new LiteralComponent(JsonDocument.Parse("[1,2,3]").RootElement),
				new StrictEqualsComponent(new VariableComponent(JsonPath.Parse("$")), new LiteralComponent(0)));

			JsonAssert.IsFalse(rule.Apply());
		}
	}
}