using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;
using TestHelpers;

namespace Json.Logic.Tests.Suite;

public class MoreTests
{
	public static IEnumerable<TestCaseData> Suite()
	{
		var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files\\more-tests.json").AdjustForPlatform();

		var content = File.ReadAllText(testsPath);

		var testSuite = JsonSerializer.Deserialize(content, TestSerializerContext.Default.TestSuite);

		return testSuite!.Tests.Select(t => new TestCaseData(t) { TestName = $"{t.Logic}  |  {t.Data.AsJsonString()}  |  {t.Expected.AsJsonString()}" });
	}

	[TestCaseSource(nameof(Suite))]
	public void Run(Test test)
	{
		var rule = JsonSerializer.Deserialize(test.Logic, TestSerializerContext.Default.Rule);

		if (rule == null)
		{
			Assert.That(test.Expected, Is.Null);
			return;
		}

		JsonAssert.AreEquivalent(test.Expected, rule.Apply(test.Data));
	}
}