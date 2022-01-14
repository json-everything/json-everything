using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class GithubTests
	{
		[Test]
		public void Issue183_RuleEvaluatesWrong_Truthy()
		{
			var logic = JsonLogic.If(
				JsonLogic.Variable("data.sub"),
				JsonLogic.StrictEquals(
					JsonLogic.Variable("data.sub.0.element"),
					"12345"
				),
				true
			);
			var json = JsonDocument.Parse(@"{
   ""data"": {
    ""sub"": [
      {
        ""element"": ""12345""
      }
    ]
  }
}").RootElement;

			var result = logic.Apply(json);

			JsonAssert.AreEquivalent(true, result);
		}
		[Test]
		public void Issue183_RuleEvaluatesWrong_Falsy()
		{
			var logic = JsonLogic.If(
				JsonLogic.Variable("data.sub"),
				JsonLogic.StrictEquals(
					JsonLogic.Variable("data.sub.0.element"),
					"12345"
				),
				true
			);
			var json = JsonDocument.Parse(@"{
   ""data"": {
    ""sub"": [
      {
        ""element"": ""12346""
      }
    ]
  }
}").RootElement;

			var result = logic.Apply(json);

			JsonAssert.AreEquivalent(false, result);
		}

		[Test]
		public void Issue183_RuleEvaluatesWrong2_Falsy()
		{
			var jsonRule = "{\"and\":[{\"if\":[{\"var\":\"data.r.0\"},{\"in\":[{\"var\":\"data.r.0.tg\"},[\"140539006\"]]},true]},{\"if\":[{\"var\":\"data.t.0\"},{\"in\":[{\"var\":\"data.t.0.tg\"},[\"140539006\"]]},true]},{\"if\":[{\"var\":\"data.v.0\"},{\"in\":[{\"var\":\"data.v.0.tg\"},[\"140539006\"]]},true]}]}";
			var logic = JsonDocument.Parse(jsonRule);
			var rule = JsonSerializer.Deserialize<Rule>(logic.RootElement.ToJsonString());


			var data = JsonDocument.Parse("{\"data\":{\"r\":[{\"tg\":\"140539006\"}],\"t\":[{\"tg\":\"140539006\"}],\"v\":[{\"tg\":\"Test\"}]}}");

			var result = rule.Apply(data.RootElement);

			JsonAssert.AreEquivalent(false, result);
		}

		[Test]
		public void Issue183_RuleEvaluatesWrong3_Falsy()
		{
			var jsonRule = "{\"===\":[{\"reduce\":[[{\"var\":\"data.r\"},{\"var\":\"data.t\"},{\"var\":\"data.v\"}],{\"\\u002B\":[{\"var\":\"accumulator\"},{\"if\":[{\"var\":\"current.0\"},1,0]}]},0]},1]}";
			var logic = JsonDocument.Parse(jsonRule);
			var rule = JsonSerializer.Deserialize<Rule>(logic.RootElement.ToJsonString());


			var data = JsonDocument.Parse("{\"data\":{\"r\":[{\"tg\":\"140539006\"},{\"tg\":\"140539006\"}]}}");

			var result = rule.Apply(data.RootElement);

			JsonAssert.AreEquivalent(true, result);
		}
	}
}
