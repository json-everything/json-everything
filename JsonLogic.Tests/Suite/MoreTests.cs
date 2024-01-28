using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests.Suite;

public class MoreTests
{
	public static IEnumerable<TestCaseData> Suite()
	{
		return Task.Run(async () =>
		{
			var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files\\more-tests.json").AdjustForPlatform();

			var content = await File.ReadAllTextAsync(testsPath);

			var testSuite = JsonSerializer.Deserialize<TestSuite>(content);

			return testSuite!.Tests.Select(t => new TestCaseData(t) { TestName = $"{t.Logic}  |  {t.Data.AsJsonString()}  |  {t.Expected.AsJsonString()}" });
		}).Result;
	}

	[TestCaseSource(nameof(Suite))]
	public void Run(Test test)
	{
		var rule = JsonSerializer.Deserialize<Rule>(test.Logic);

		if (rule == null)
		{
			Assert.IsNull(test.Expected);
			return;
		}

		JsonAssert.AreEquivalent(test.Expected, rule.Apply(test.Data));
	}
}