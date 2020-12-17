using System.Text.Json;
using Json.Logic.Components;
using Json.Path;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class NoneTests
	{
		[Test]
		public void NoneMatchCondition()
		{
			var rule = new NoneComponent(new LiteralComponent(JsonDocument.Parse("[1,2,3]").RootElement),
				new StrictEqualsComponent(new VariableComponent(JsonPath.Parse("$")), new LiteralComponent(2)));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void SomeDoNotMatchCondition()
		{
			var rule = new NoneComponent(new LiteralComponent(JsonDocument.Parse("[1,2,3]").RootElement),
				new StrictEqualsComponent(new VariableComponent(JsonPath.Parse("$")), new LiteralComponent(0)));

			JsonAssert.IsTrue(rule.Apply());
		}
	}
}