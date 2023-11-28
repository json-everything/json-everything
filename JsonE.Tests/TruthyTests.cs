using System.Collections.Generic;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.JsonE.Tests;

public class TruthyTests
{
	public static IEnumerable<TestCaseData> TruthyTestCases
	{
		get
		{
			yield return new TestCaseData(null, false);
			yield return new TestCaseData(new JsonObject(), false);
			yield return new TestCaseData(new JsonObject { ["foo"] = 42 }, true);
			yield return new TestCaseData(new JsonArray(), false);
			yield return new TestCaseData(new JsonArray { 42 }, true);
			yield return new TestCaseData((JsonNode?)string.Empty, false);
			yield return new TestCaseData((JsonNode?)"foo", true);
			yield return new TestCaseData((JsonNode?)0, false);
			yield return new TestCaseData((JsonNode?)1, true);
			yield return new TestCaseData((JsonNode?)false, false);
			yield return new TestCaseData((JsonNode?)true, true);
		}
	}

	[TestCaseSource(nameof(TruthyTestCases))]
	public void IsTruthy(JsonNode? node, bool expected)
	{
		Assert.AreEqual(expected, node.IsTruthy());
	}
}