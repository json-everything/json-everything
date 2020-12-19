using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests.Suite
{
	public class SuiteRunner
	{
		public static IEnumerable<TestCaseData> Suite()
		{
			return Task.Run(async () =>
			{
				using var client = new HttpClient();
				using var request = new HttpRequestMessage(HttpMethod.Get, "https://jsonlogic.com/tests.json");
				using var response = await client.SendAsync(request);

				var content = await response.Content.ReadAsStringAsync();
				var testSuite = JsonSerializer.Deserialize<TestSuite>(content);

				return testSuite.Tests.Select(t => new TestCaseData(t){TestName = $"{t.Logic}  |  {t.Data.ToJsonString()}  |  {t.Expected.ToJsonString()}"});
			}).Result;
		}

		[TestCaseSource(nameof(Suite))]
		public void Run(Test test)
		{
			var rule = JsonSerializer.Deserialize<LogicComponent>(test.Logic);

			if (rule == null)
			{
				Assert.AreEqual(JsonValueKind.Null, test.Expected.ValueKind);
				return;
			}

			JsonAssert.AreEquivalent(test.Expected, rule.Apply(test.Data));
		}
	}
}
