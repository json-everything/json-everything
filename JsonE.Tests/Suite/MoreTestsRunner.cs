using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;
using Yaml2JsonNode;

namespace Json.JsonE.Tests.Suite;

public class MoreTestsRunner
{
	private const string _testsFile = "Files/more-tests.yml";

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

		var tests = DeserializeAll<Test>(yamlText, TestEnvironment.SerializerOptions)!;

		return tests.Select(t => new TestCaseData(t) { TestName = $"{t.Title}  |  {t.Template.AsJsonString(TestEnvironment.SerializerOptions)}  |  {t.Context.AsJsonString(TestEnvironment.SerializerOptions)}" });
	}

	[TestCaseSource(nameof(Suite))]
	public void Run(Test test)
	{
		try
		{
			OutputTest(test);

			test.Context!["now"] = "2017-01-19T16:27:20.974Z";

			var result = JsonE.Evaluate(test.Template, test.Context);

			if (test.HasError)
				Assert.Fail($"Expected error: {test.Error}\nActual: {result}");

			JsonAssert.AreEquivalent(test.Expected, result);
		}
		catch (Exception e) when (e is not AssertionException)
		{
			if (!test.HasError) throw;
			if (test.Error is not null)
			{
				var errorMessage = $"{GetErrorPrefix(e)}: {e.Message}";
				if (test.Error != errorMessage && test.Error.EndsWith(e.Message))
					Assert.Inconclusive($"Error message is correct, but received type ({GetErrorPrefix(e)}) is wrong.\n");

				Assert.AreEqual(test.Error, errorMessage);
			}
		}
	}

	private static string GetErrorPrefix(Exception exception)
	{
		if (exception is TemplateException) return "TemplateError";
		if (exception is InterpreterException) return "InterpreterError";

		return string.Empty;
	}

	private static void OutputTest(Test test)
	{
		Console.WriteLine();
		Console.WriteLine($"Title:    {test.Title}");
		Console.WriteLine($"Template: {test.Template.AsJsonString(TestEnvironment.SerializerOptions)}");
		Console.WriteLine($"Context:  {test.Context.AsJsonString(TestEnvironment.SerializerOptions)}");
		if (test.Expected is not null)
			Console.WriteLine($"Result:   {test.Expected.AsJsonString(TestEnvironment.SerializerOptions)}");
		if (test.HasError)
			Console.WriteLine($"Error:    {test.ErrorNode.AsJsonString(TestEnvironment.SerializerOptions)}");
	}
}
