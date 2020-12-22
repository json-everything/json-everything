using System.Text.Json;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class SomeTests
	{
		[Test]
		public void SomeMatchCondition()
		{
			var rule = new SomeRule(JsonDocument.Parse("[1,2,3]").RootElement,
				new StrictEqualsRule(new VariableRule(""), 2));

			JsonAssert.IsTrue(rule.Apply());
		}

		[Test]
		public void SomeDoNotMatchCondition()
		{
			var rule = new SomeRule(JsonDocument.Parse("[1,2,3]").RootElement,
				new StrictEqualsRule(new VariableRule(""), 0));

			JsonAssert.IsFalse(rule.Apply());
		}
	}
}