using System.Text.Json;
using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class NoneTests
	{
		[Test]
		public void NoneMatchCondition()
		{
			var rule = new NoneComponent(JsonDocument.Parse("[1,2,3]").RootElement,
				new StrictEqualsComponent(new VariableComponent(""), 2));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void SomeDoNotMatchCondition()
		{
			var rule = new NoneComponent(JsonDocument.Parse("[1,2,3]").RootElement,
				new StrictEqualsComponent(new VariableComponent(""), 0));

			JsonAssert.IsTrue(rule.Apply());
		}
	}
}