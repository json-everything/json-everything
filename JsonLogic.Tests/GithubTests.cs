using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Logic.Rules;
using Json.More;
using NUnit.Framework;

using static Json.Logic.JsonLogic;

namespace Json.Logic.Tests;

public class GithubTests
{
	[Test]
	public void Issue183_RuleEvaluatesWrong_Truthy()
	{
		var logic = If(
			Variable("data.sub"),
			StrictEquals(
				Variable("data.sub.0.element"),
				"12345"
			),
			true
		);
		var json = JsonNode.Parse(@"{
   ""data"": {
    ""sub"": [
      {
        ""element"": ""12345""
      }
    ]
  }
}");

		var result = logic.Apply(json);

		JsonAssert.AreEquivalent(true, result);
	}
	[Test]
	public void Issue183_RuleEvaluatesWrong_Falsy()
	{
		var logic = If(
			Variable("data.sub"),
			StrictEquals(
				Variable("data.sub.0.element"),
				"12345"
			),
			true
		);
		var json = JsonNode.Parse(@"{
   ""data"": {
    ""sub"": [
      {
        ""element"": ""12346""
      }
    ]
  }
}");

		var result = logic.Apply(json);

		JsonAssert.AreEquivalent(false, result);
	}

	[Test]
	public void Issue183_RuleEvaluatesWrong2_Falsy()
	{
		var jsonRule = "{\"and\":[{\"if\":[{\"var\":\"data.r.0\"},{\"in\":[{\"var\":\"data.r.0.tg\"},[\"140539006\"]]},true]},{\"if\":[{\"var\":\"data.t.0\"},{\"in\":[{\"var\":\"data.t.0.tg\"},[\"140539006\"]]},true]},{\"if\":[{\"var\":\"data.v.0\"},{\"in\":[{\"var\":\"data.v.0.tg\"},[\"140539006\"]]},true]}]}";
		var logic = JsonNode.Parse(jsonRule);
		var rule = logic.Deserialize<Rule>();


		var data = JsonNode.Parse("{\"data\":{\"r\":[{\"tg\":\"140539006\"}],\"t\":[{\"tg\":\"140539006\"}],\"v\":[{\"tg\":\"Test\"}]}}");

		var result = rule!.Apply(data);

		JsonAssert.AreEquivalent(false, result);
	}

	[Test]
	public void Issue183_RuleEvaluatesWrong3_Falsy()
	{
		var jsonRule = "{\"===\":[{\"reduce\":[[{\"var\":\"data.r\"},{\"var\":\"data.t\"},{\"var\":\"data.v\"}],{\"\\u002B\":[{\"var\":\"accumulator\"},{\"if\":[{\"var\":\"current.0\"},1,0]}]},0]},1]}";
		var logic = JsonNode.Parse(jsonRule);
		var rule = logic.Deserialize<Rule>();


		var data = JsonNode.Parse("{\"data\":{\"r\":[{\"tg\":\"140539006\"},{\"tg\":\"140539006\"}]}}");

		var result = rule!.Apply(data);

		JsonAssert.AreEquivalent(true, result);
	}

	[Test]
	public void Issue263_SomeInTest()
	{
		var rule = Some(
			Variable("x"),
			In(
				Variable(""),
				Variable("y")
			)
		);

		var data = JsonNode.Parse("{\"x\":[-1, 0, 1],\"y\":[2, 3, 1]}");

		JsonAssert.IsTrue(rule.Apply(data));
	}

	[Test]
	public void Issue286_InShouldReturnFalseForNonArray()
	{
		var rule = JsonSerializer.Deserialize<Rule>("{ \"in\" : [ {\"var\": \"item\"}, {\"var\": \"list\"} ] }");
		var result = rule!.Apply(new JsonObject { ["some_item"] = 123 });

		JsonAssert.IsFalse(result);
	}

	[Test]
	public void Pull303_CustomConverters()
	{
		var rule = JsonSerializer.Deserialize<Rule>("{ \"+\" : [ 1, 2 ] }");

		Assert.IsInstanceOf<AddRule>(rule);
		Assert.IsTrue(rule!.Apply().IsEquivalentTo(3));
	}

	[Test]
	public void Issue313_VarEqualsNull()
	{
		var node = JsonNode.Parse(@"{
  ""=="": [
    {""var"": ""value""},
    null
  ]
}");

		var rule = node.Deserialize<Rule>();
		var result = rule!.Apply(JsonNode.Parse("{\"value\": null}"));

		Assert.IsTrue(result.IsEquivalentTo(true));
	}

	[Test]
	public void Issue318_CanParseStringAsFloatingPointNumberInAnyCulture()
	{
		var culture = CultureInfo.CurrentCulture;

		try
		{
			CultureInfo.CurrentCulture = new CultureInfo("de-AT");
			var number = JsonValue.Create("3.14").Numberify();

			Assert.AreEqual(3.14d, number);
		}
		finally
		{
			CultureInfo.CurrentCulture = culture;
		}
	}

	[Test]
	public void Issue383_NoneUsesLocalValueForVarResolution()
	{
		var rule = None(
			Variable("additionalDrivers"),
			StrictEquals(
				Variable("relationshipToProposer.dataCode"),
				"J"
			)
		);

		var data = JsonNode.Parse(@"{
	""hasAdditionalDrivers"": true,
	""additionalDrivers"": [
		{}
	],
	""relationshipToProposer"": {
		""dataCode"": ""J"",
		""displayText"": null
	}
}");

		var result = rule.Apply(data);

		Assert.IsTrue(result.IsEquivalentTo(true));
	}
}