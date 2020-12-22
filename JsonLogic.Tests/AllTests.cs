using System.Text.Json;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class AllTests
	{
		[Test]
		public void AllMatchCondition()
		{
			var rule = new AllRule(JsonDocument.Parse("[1,2,3]").RootElement,
				new MoreThanRule(new VariableRule(""), 0));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void AllDoNotMatchCondition()
		{
			var rule = new AllRule(JsonDocument.Parse("[1,-2,3]").RootElement,
				new MoreThanRule(new VariableRule(""), 0));

			JsonAssert.IsFalse(rule.Apply());
		}
	}
}
