using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;
using Yaml2JsonNode;

namespace Json.JsonE.Tests.Suite;

public class SuiteRunner
{
	private const string _testsFile = "../../../../ref-repos/json-e/specification.yml";

	private static IEnumerable<T>? DeserializeAll<T>(string yamlText, JsonSerializerOptions? options = null)
	{
		var yaml = YamlSerializer.Parse(yamlText);
		var json = yaml.ToJsonNode().Where(x => x!["title"] is not null).ToJsonArray();

		return json.Deserialize<T[]>(options);
	}

	public static IEnumerable<TestCaseData> Suite()
	{
		var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, _testsFile);

		var yamlText = File.ReadAllText(testsPath);

		var tests = DeserializeAll<Test>(yamlText)!;

		return tests.Select(t => new TestCaseData(t) { TestName = $"{t.Title}  |  {t.Template.AsJsonString()}  |  {t.Context.AsJsonString()}" });
	}

	[TestCaseSource(nameof(Suite))]
	public void Run(Test test)
	{
		try
		{
			OutputTest(test);

			var template = test.Template.Deserialize<JsonETemplate>()!;
			var result = template.Evaluate(test.Context);

			if (!test.HasError)
			{
				Assert.Fail($"Expected error: {test.Error}");
			}

			JsonAssert.AreEquivalent(test.Expected, result);
		}
		catch
		{
			if (!test.HasError) throw;
		}
	}

	private static void OutputTest(Test test)
	{
		Console.WriteLine($"Template: {test.Template.AsJsonString()}");
		Console.WriteLine($"Context:  {test.Context.AsJsonString()}");
		if (test.Expected is not null)
			Console.WriteLine($"Result:   {test.Expected.AsJsonString()}");
		if (test.HasError)
			Console.WriteLine($"Error:    {test.ErrorNode.AsJsonString()}");
	}
}