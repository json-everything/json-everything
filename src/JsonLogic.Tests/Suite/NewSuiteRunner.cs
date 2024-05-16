using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelpers;

namespace Json.Logic.Tests.Suite;

public class NewSuiteRunner
{
	public static IEnumerable<TestCaseData> Suite()
	{
		var text = Task.Run(async () =>
		{
			var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files\\tests.json").AdjustForPlatform();

			string? content = null;
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
			return content;

		}).Result;

		var testSuite = JsonSerializer.Deserialize(text, TestSerializerContext.Default.TestSuite);

		return testSuite!.Tests.Select(t => new TestCaseData(t) { TestName = $"{t.Logic}  |  {More.JsonNodeExtensions.AsJsonString(t.Data)}  |  {More.JsonNodeExtensions.AsJsonString(t.Expected)}" });
	}

	[TestCaseSource(nameof(Suite))]
	public void Run(Test test)
	{
		var rule = JsonNode.Parse(test.Logic);

		var result = JsonLogic.Apply(rule, test.Data);

		JsonAssert.AreEquivalent(test.Expected, result);
	}
}