using System;
using System.Collections.Generic;
using System.IO;
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
				var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files\\tests.json").AdjustForPlatform();

				string content = null;
				try
				{
					using var client = new HttpClient();
					using var request = new HttpRequestMessage(HttpMethod.Get, "https://jsonlogic.com/tests.json");
					using var response = await client.SendAsync(request);
					
					content = await response.Content.ReadAsStringAsync();
					
					await File.WriteAllTextAsync(testsPath, content);
				}
				catch (Exception e)
				{
					content ??= await File.ReadAllTextAsync(testsPath);

					Console.WriteLine(e);
				}


				var testSuite = JsonSerializer.Deserialize<TestSuite>(content);

				return testSuite!.Tests.Select(t => new TestCaseData(t){TestName = $"{t.Logic}  |  {t.Data.ToJsonString()}  |  {t.Expected.ToJsonString()}"});
			}).Result;
		}

		[TestCaseSource(nameof(Suite))]
		public void Run(Test test)
		{
			var rule = JsonSerializer.Deserialize<Rule>(test.Logic);

			if (rule == null)
			{
				Assert.AreEqual(JsonValueKind.Null, test.Expected.ValueKind);
				return;
			}

			JsonAssert.AreEquivalent(test.Expected, rule.Apply(test.Data));
		}
	}
}
