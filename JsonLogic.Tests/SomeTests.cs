using System.Text.Json;
using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class SomeTests
	{
		[Test]
		public void SomeMatchCondition()
		{
			var rule = new SomeComponent(JsonDocument.Parse("[1,2,3]").RootElement,
				new StrictEqualsComponent(new VariableComponent(""), 2));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void SomeDoNotMatchCondition()
		{
			var rule = new SomeComponent(JsonDocument.Parse("[1,2,3]").RootElement,
				new StrictEqualsComponent(new VariableComponent(""), 0));

			JsonAssert.IsFalse(rule.Apply());
		}
	}
}