using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;
using TestHelpers;

namespace Json.Logic.Tests;

public class NoneTests
{
	[Test]
	public void NoneMatchCondition()
	{
		var rule = new NoneRule(JsonNode.Parse("[1,2,3]"),
			new StrictEqualsRule(new VariableRule(""), 2));

		JsonAssert.IsFalse(rule.Apply());
	}

	[Test]
	public void SomeDoNotMatchCondition()
	{
		var rule = new NoneRule(JsonNode.Parse("[1,2,3]"),
			new StrictEqualsRule(new VariableRule(""), 0));

		JsonAssert.IsTrue(rule.Apply());
	}
}