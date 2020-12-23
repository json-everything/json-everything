using System.Text.Json;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class NoneTests
	{
		[Test]
		public void NoneMatchCondition()
		{
			var rule = new NoneRule(JsonDocument.Parse("[1,2,3]").RootElement,
				new StrictEqualsRule(new VariableRule(""), 2));

			JsonAssert.IsFalse(rule.Apply());
		}

		[Test]
		public void SomeDoNotMatchCondition()
		{
			var rule = new NoneRule(JsonDocument.Parse("[1,2,3]").RootElement,
				new StrictEqualsRule(new VariableRule(""), 0));

			JsonAssert.IsTrue(rule.Apply());
		}
	}
}