using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests.Suite;

public class Spelling
{
	public static IEnumerable<TestCaseData> Suite()
	{
		var text = Task.Run(async () =>
		{
			var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files\\tests.json").AdjustForPlatform();

			string content = null!;
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

		var testSuite = JsonSerializer.Deserialize<TestSuite>(text, TestSerializerContext.OptionsManager.SerializerOptions);

		return testSuite!.Tests.Select(t => new TestCaseData(t) { TestName = $"{t.Logic}  |  {t.Data.AsJsonString()}  |  {t.Expected.AsJsonString()}" });
	}

	[TestCaseSource(nameof(Suite))]
	public void Run(Test test)
	{
		var node = JsonNode.Parse(test.Logic);
		var rule = JsonSerializer.Deserialize<Rule>(test.Logic, TestSerializerContext.OptionsManager.SerializerOptions);

		var serialized = JsonSerializer.SerializeToNode(rule, TestSerializerContext.OptionsManager.SerializerOptions);

		if (node.IsEquivalentTo(serialized)) return;

		Console.WriteLine($"Expected: {node.AsJsonString(TestSerializerContext.OptionsManager.SerializerOptions)}");
		Console.WriteLine($"Actual:   {serialized.AsJsonString(TestSerializerContext.OptionsManager.SerializerOptions)}");
		Assert.Inconclusive();
	}
}