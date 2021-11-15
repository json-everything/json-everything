using System.Text.Json;
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
	}
}
