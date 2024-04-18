using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Logic.Tests.Suite;

public class NewMoreTests
{
	public static IEnumerable<TestCaseData> Suite()
	{
		return Task.Run(async () =>
		{
			var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files\\more-tests.json").AdjustForPlatform();

			var content = await File.ReadAllTextAsync(testsPath);

			var testSuite = JsonSerializer.Deserialize(content, TestSerializerContext.Default.TestSuite);

			return testSuite!.Tests.Select(t => new TestCaseData(t) { TestName = $"{t.Logic}  |  {More.JsonNodeExtensions.AsJsonString(t.Data)}  |  {More.JsonNodeExtensions.AsJsonString(t.Expected)}" });
		}).Result;
	}

	[TestCaseSource(nameof(Suite))]
	public void Run(Test test)
	{
		var rule = JsonNode.Parse(test.Logic);

		var result = JsonLogic.Apply(rule, test.Data);

		JsonAssert.AreEquivalent(test.Expected, result);
	}
}