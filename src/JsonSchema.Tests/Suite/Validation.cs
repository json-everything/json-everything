using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests.Suite;

public class Validation
{
	private const string _testCasesPath = @"../../../../../ref-repos/JSON-Schema-Test-Suite/tests";
	private const string _remoteSchemasPath = @"../../../../../ref-repos/JSON-Schema-Test-Suite/remotes";

	private const bool _useExternal = false;
	private const bool _runDraftNext = true;
	private const string _externalTestCasesPath = @"../../../../../../JSON-Schema-Test-Suite/tests";
	private const string _externalRemoteSchemasPath = @"../../../../../../JSON-Schema-Test-Suite/remotes";

	public static IEnumerable<TestCaseData> TestCases()
	{
		return GetTests("draft6")
			.Concat(GetTests("draft7"))
			.Concat(GetTests("draft2019-09"))
			.Concat(GetTests("draft2020-12"))
			.Concat(_runDraftNext ? GetTests("v1") : []);
	}

	private static IEnumerable<TestCaseData> GetTests(string draftFolder)
	{
		// ReSharper disable once HeuristicUnreachableCode
		var testCasesPath = _useExternal ? _externalTestCasesPath : _testCasesPath;

		var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, testCasesPath, $"{draftFolder}/")
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath)) return [];

		var fileNames = Directory.GetFiles(testsPath, "*.json", SearchOption.AllDirectories);
		var evaluationOptions = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
		};

		var allTests = new List<TestCaseData>();
		foreach (var fileName in fileNames)
		{
			var shortFileName = Path.GetFileNameWithoutExtension(fileName);

			// adjust for format
			evaluationOptions.RequireFormatValidation = fileName.Contains("format/".AdjustForPlatform()) &&
											  // uri-template will throw an exception as it's explicitly unsupported
											  shortFileName != "uri-template";

			var contents = File.ReadAllText(fileName);
			var collections = JsonSerializer.Deserialize<List<TestCollection>>(contents, TestEnvironment.TestSuiteSerializationOptions);

			foreach (var collection in collections!)
			{
				collection.IsOptional = fileName.Contains("optional");
				foreach (var test in collection.Tests)
				{
					var keywords = draftFolder switch
					{
						"draft6" => Dialect.Draft06,
						"draft7" => Dialect.Draft07,
						"draft2019-09" => Dialect.Draft201909,
						"draft2020-12" => Dialect.Draft202012,
						"v1" => Dialect.V1,
						_ => throw new ArgumentOutOfRangeException(nameof(draftFolder), $"{draftFolder} is unsupported")
					};

					if (fileName.Contains("format/".AdjustForPlatform()))
					{
						if (shortFileName == "uri-template") continue; // uri-template will throw an exception as it's explicitly unsupported

						keywords = keywords.UseFormatValidation();
					}

					var buildOptions = new BuildOptions
					{
						KeywordRegistry = keywords,
						SchemaRegistry = new()
					};
					var optional = collection.IsOptional ? "(optional) / " : null;
					var name = $"{draftFolder} / {optional}{shortFileName} / {collection.Description} / {test.Description}";
					var evaluationOptionsCopy = EvaluationOptions.From(evaluationOptions);
					allTests.Add(new TestCaseData(collection, test, shortFileName, buildOptions, evaluationOptionsCopy) { TestName = name });
				}
			}
		}

		return allTests;
	}

	[OneTimeSetUp]
	public void LoadRemoteSchemas()
	{
		// ReSharper disable once HeuristicUnreachableCode
		var remotesPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, _useExternal ? _externalRemoteSchemasPath : _remoteSchemasPath)
			.AdjustForPlatform();
		if (!Directory.Exists(remotesPath)) throw new Exception("Cannot find remotes folder");

		var fileNames = Directory.GetFiles(remotesPath, "*.json", SearchOption.AllDirectories);

		foreach (var fileName in fileNames)
		{
			try
			{
				var uri = new Uri(fileName.Replace(remotesPath, "http://localhost:1234").Replace('\\', '/'));
				var schema = JsonSchema.FromFile(fileName, baseUri: uri);
				SchemaRegistry.Global.Register(uri, schema); // it seems a number of remotes have `$id`s different from their file path
			}
			catch (JsonSchemaException e)
			{
				TestConsole.WriteLine($"Error loading file '{fileName}'");
			}
		}
	}

	[TestCaseSource(nameof(TestCases))]
	public void Test(TestCollection collection, TestCase test, string fileName, BuildOptions buildOptions, EvaluationOptions evaluationOptions)
	{
		TestConsole.WriteLine();
		TestConsole.WriteLine();
		TestConsole.WriteLine(fileName);
		TestConsole.WriteLine(collection.Description);
		TestConsole.WriteLine(test.Description);
		TestConsole.WriteLine(test.Valid ? "valid" : "invalid");
		TestConsole.WriteLine();
		TestConsole.WriteLine(JsonSerializer.Serialize(collection.Schema, TestEnvironment.TestOutputSerializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(JsonSerializer.Serialize(test.Data, TestEnvironment.TestOutputSerializerOptions));
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

			throw;
		}

		//result.ToBasic();
		TestConsole.WriteLine();
		TestConsole.WriteLine("Result:");
		TestConsole.WriteLine(JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions));

		if (collection.IsOptional && result.IsValid != test.Valid)
			Assert.Inconclusive("Test optional");
		Assert.That(result.IsValid, Is.EqualTo(test.Valid));
	}

	[Test]
	public void EnsureTestSuiteConfiguredForServerBuild()
	{
		Assert.That(_useExternal, Is.False);
		//Assert.IsFalse(_runDraftNext);
	}
}