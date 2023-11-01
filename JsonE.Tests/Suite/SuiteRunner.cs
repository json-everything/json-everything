using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.More;
using NUnit.Framework;
using Yaml2JsonNode;

namespace Json.JsonE.Tests.Suite;

public class SuiteRunner
{
	private const string _testsFile = @"../../../../ref-repos/json-e/specification.yml";

	public static IEnumerable<TestCaseData> Suite()
	{
		var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, _testsFile).AdjustForPlatform();

		var yamlText = File.ReadAllText(testsPath);

		var testSuite = YamlSerializer.Deserialize<TestSuite>(yamlText);

		return testSuite!.Tests.Select(t => new TestCaseData(t) { TestName = $"{t.Logic}  |  {t.Data.AsJsonString()}  |  {t.Expected.AsJsonString()}" });
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

	private static readonly JsonSerializerOptions _spellingTestSerializerOptions =
		new()
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			Converters = { new LogicComponentConverter { SaveSource = false } }
		};

	[TestCaseSource(nameof(Suite))]
	public void SpellingTest(Test test)
	{
		var node = JsonNode.Parse(test.Logic);
		var rule = JsonSerializer.Deserialize<Rule>(test.Logic, _spellingTestSerializerOptions);

		var serialized = JsonSerializer.SerializeToNode(rule);

		if (node.IsEquivalentTo(serialized)) return;

		Console.WriteLine($"Expected: {node.AsJsonString(_spellingTestSerializerOptions)}");
		Console.WriteLine($"Actual:   {serialized.AsJsonString(_spellingTestSerializerOptions)}");
		Assert.Inconclusive();
	}


}