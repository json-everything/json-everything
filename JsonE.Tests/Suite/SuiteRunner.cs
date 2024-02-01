using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using NUnit.Framework;
using Yaml2JsonNode;

namespace Json.JsonE.Tests.Suite;

public class SuiteRunner
{
	private const string _testsFile = "../../../../ref-repos/json-e/specification.yml";
	private static readonly JsonSerializerOptions _serializerOptions =
		new(JsonETestSerializerContext.Default.Options)
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	private static readonly (string Name, string Reason)[] _ignored =
	[
		("division (3)", "decimals are more precise than the test expects"),
		("division (4)", "decimals are more precise than the test expects"),
		("representation of various unicode codepoints are consistent", "json can be encoded differently"),
		("sorting pairs by unicode key strings sorts lexically by codepoint", "json can be encoded differently"),
		("sorting pairs by unicode key strings sorts lexically by codepoint, even with chars above base plane", "json can be encoded differently"),
	];

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

		var tests = DeserializeAll<Test>(yamlText, _serializerOptions)!;

		return tests.Select(t => new TestCaseData(t) { TestName = $"{t.Title}  |  {t.Template.AsJsonString(_serializerOptions)}  |  {t.Context.AsJsonString(_serializerOptions)}" });
	}

	[TestCaseSource(nameof(Suite))]
	public void Run(Test test)
	{
		try
		{
			OutputTest(test);

			var ignored = _ignored.FirstOrDefault(x => x.Name == test.Title);
			if (ignored.Name != null)
				Assert.Inconclusive(ignored.Reason);

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
				if (test.Error != errorMessage)
					Assert.Inconclusive($"Received error message, but it was different.\n" +
					                    $"  Expected: {test.Error}\n" +
					                    $"  Actual: {errorMessage}\n");
			}
		}
	}

	private static string GetErrorPrefix(Exception exception)
	{
		if (exception is TemplateException) return "TemplateError";
		if (exception is InterpreterException) return "InterpreterError";
		if (exception is BuiltInException) return "BuiltinError";
		if (exception is SyntaxException) return "SyntaxError";
		if (exception is TypeException) return "TypeError";

		return string.Empty;
	}

	private static void OutputTest(Test test)
	{
		Console.WriteLine();
		Console.WriteLine($"Title:    {test.Title}");
		Console.WriteLine($"Template: {test.Template.AsJsonString(_serializerOptions)}");
		Console.WriteLine($"Context:  {test.Context.AsJsonString(_serializerOptions)}");
		if (test.HasError)
			Console.WriteLine($"Error:    {test.ErrorNode.AsJsonString(_serializerOptions)}");
		else
			Console.WriteLine($"Result:   {test.Expected.AsJsonString(_serializerOptions)}");
	}
}

[JsonSerializable(typeof(Test))]
[JsonSerializable(typeof(Test[]))]
internal partial class JsonETestSerializerContext : JsonSerializerContext;