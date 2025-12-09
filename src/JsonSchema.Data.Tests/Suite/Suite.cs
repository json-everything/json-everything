using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Humanizer;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Data.Tests.Suite;

public class Suite
{
	private const string _testCasesPath = @"../../../../../ref-repos/json-schema-vocab-test-suites/tests/data";
	private const string _remoteSchemasPath = _testCasesPath + "/external-sources";
	private const string _coreSchemasPath = _testCasesPath + "/core";

	private const bool _useExternal = false;
	private const string _externalTestCasesPath = @"../../../../../../json-schema-vocab-test-suites/tests/data";
	private const string _externalRemoteSchemasPath = _externalTestCasesPath + "/external-sources";
	private const string _externalCoreSchemasPath = _externalTestCasesPath + "/core";

	public static IEnumerable<TestCaseData> TestCases()
	{
		// ReSharper disable once HeuristicUnreachableCode
		var testCasesPath = _useExternal ? _externalTestCasesPath : _testCasesPath;

		var testsPath = System.IO.Path.Combine(TestContext.CurrentContext.WorkDirectory, testCasesPath)
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath)) return [];

		var fileNames = Directory.GetFiles(testsPath, "*.json");
		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};

		var allTests = new List<TestCaseData>();
		foreach (var fileName in fileNames)
		{
			var shortFileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
			var contents = File.ReadAllText(fileName);
			var collections = JsonSerializer.Deserialize<List<TestCollection>>(contents, TestEnvironment.SerializerOptions);

			foreach (var collection in collections!)
			{
				collection.IsOptional = fileName.Contains("optional");
				foreach (var test in collection.Tests)
				{
					var buildOptions = new BuildOptions
					{
						Dialect = Dialect.Data_202012,
						SchemaRegistry = new()
					};
					var optional = collection.IsOptional ? "(optional)/" : null;
					var name = $"{shortFileName}/{optional}{collection.Description.Kebaberize()}/{test.Description.Kebaberize()}";
					var evaluationOptionsCopy = EvaluationOptions.From(options);
					allTests.Add(new TestCaseData(collection, test, shortFileName, buildOptions, evaluationOptionsCopy) { TestName = name });
				}
			}
		}

		return allTests;
	}

	[OneTimeSetUp]
	public static void LoadRemoteSchemas()
	{
		var remotesPath = _useExternal ? _externalRemoteSchemasPath : _remoteSchemasPath;

		var remotesFilePath = System.IO.Path.Combine(TestContext.CurrentContext.WorkDirectory, remotesPath)
			.AdjustForPlatform();
		if (!Directory.Exists(remotesFilePath))
			throw new Exception("cannot find remotes path");

		BuildOptions.Default.GetDataRegistry().Fetch = uri =>
		{
			if (uri.OriginalString.StartsWith("https://json-everything.lib")) return default;
			var filePath = uri.OriginalString.Replace("http://localhost:1234", remotesPath).Split("#")[0];
			var text = File.ReadAllText(filePath);
			return JsonDocument.Parse(text).RootElement;
		};
	}

	[TestCaseSource(nameof(TestCases))]
	public void Test(TestCollection collection, TestCase test, string fileName, BuildOptions buildOptions, EvaluationOptions evaluationOptions)
	{
		TestConsole.WriteLine();
		TestConsole.WriteLine();
		TestConsole.WriteLine(fileName);
		TestConsole.WriteLine(collection.Description);
		TestConsole.WriteLine(test.Description);
		if (test.Error)
			TestConsole.WriteLine("expected error");
		else
			TestConsole.WriteLine(test.Valid ? "valid" : "invalid");
		TestConsole.WriteLine();
		TestConsole.WriteLine(JsonSerializer.Serialize(collection.Schema, TestEnvironment.SerializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(JsonSerializer.Serialize(test.Data, TestEnvironment.SerializerOptions));
		TestConsole.WriteLine();

		JsonSchema schema;
		try
		{
			schema = Measure.Run("Build", () => JsonSchema.Build(collection.Schema, buildOptions));
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			if (collection.IsOptional)
			{
				Assert.Inconclusive();
				return;
			}

			if (test.Error) return;

			throw;
		}

		EvaluationResults result;
		try
		{
			result = Measure.Run("Evaluate", () => schema.Evaluate(test.Data, evaluationOptions));
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			if (collection.IsOptional)
			{
				Assert.Inconclusive();
				return;
			}

			if (test.Error) return;

			throw;
		}
		//result.ToBasic();
		TestConsole.WriteLine(JsonSerializer.Serialize(result, TestEnvironment.SerializerOptions));

		if (collection.IsOptional && result.IsValid != test.Valid)
			Assert.Inconclusive("Test optional");
		Assert.That(result.IsValid, Is.EqualTo(test.Valid));
	}

	public static IEnumerable<TestCaseData> CoreTestCases()
	{
		// ReSharper disable once HeuristicUnreachableCode
		var testCasesPath = _useExternal ? _externalCoreSchemasPath : _coreSchemasPath;

		var testsPath = System.IO.Path.Combine(TestContext.CurrentContext.WorkDirectory, testCasesPath)
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath)) return [];

		var fileNames = Directory.GetFiles(testsPath, "*.json");

		var allTests = new List<TestCaseData>();
		foreach (var fileName in fileNames)
		{
			var shortFileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
			var contents = File.ReadAllText(fileName);

			var name = $"${shortFileName}";
			allTests.Add(new TestCaseData(contents) { TestName = name });
		}

		return allTests;
	}

	[TestCaseSource(nameof(CoreTestCases))]
	public void CoreKeywordsAreInvalid(string schemaText)
	{
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<JsonSchema>(schemaText, TestEnvironment.SerializerOptions));
	}

	[Test]
	public void EnsureTestSuiteConfiguredForServerBuild()
	{
		Assert.That(_useExternal, Is.False);
	}
}