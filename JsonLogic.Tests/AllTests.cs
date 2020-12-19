using System.Text.Json;
using Json.Logic.Components;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class AllTests
	{
		[Test]
		public void AllMatchCondition()
		{
			var rule = new AllComponent(JsonDocument.Parse("[1,2,3]").RootElement,
				new MoreThanComponent(new VariableComponent(""), 0));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void AllDoNotMatchCondition()
		{
			var rule = new AllComponent(JsonDocument.Parse("[1,-2,3]").RootElement,
				new MoreThanComponent(new VariableComponent(""), 0));

			JsonAssert.IsFalse(rule.Apply());
		}
	}
}
