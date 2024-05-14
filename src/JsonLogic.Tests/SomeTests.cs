using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests;

public class SomeTests
{
	[Test]
	public void SomeMatchCondition()
	{
		var rule = new SomeRule(JsonNode.Parse("[1,2,3]"),
			new StrictEqualsRule(new VariableRule(""), 2));

		JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void SomeDoNotMatchCondition()
	{
		var rule = new SomeRule(JsonNode.Parse("[1,2,3]"),
			new StrictEqualsRule(new VariableRule(""), 0));

		JsonAssert.IsFalse(rule.Apply());
	}
}