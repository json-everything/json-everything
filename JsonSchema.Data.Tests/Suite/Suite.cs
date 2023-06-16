using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Humanizer;
using Json.More;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.Data.Tests.Suite;

public class Suite
{
	private const string _testCasesPath = @"../../../../ref-repos/json-schema-vocab-test-suites/tests/data";
	private const string _remoteSchemasPath = _testCasesPath + "/external-sources";
	private const string _coreSchemasPath = _testCasesPath + "/core";

	private const bool _useExternal = false;
	private const string _externalTestCasesPath = @"../../../../../json-schema-vocab-test-suites/tests/data";
	private const string _externalRemoteSchemasPath = _externalTestCasesPath + "/external-sources";
	private const string _externalCoreSchemasPath = _externalTestCasesPath + "/core";

	public static IEnumerable<TestCaseData> TestCases()
	{
		// ReSharper disable once HeuristicUnreachableCode
		var testCasesPath = _useExternal ? _externalTestCasesPath : _testCasesPath;

		var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, testCasesPath)
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath)) return Enumerable.Empty<TestCaseData>();

		var fileNames = Directory.GetFiles(testsPath, "*.json");
		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};

		var allTests = new List<TestCaseData>();
		foreach (var fileName in fileNames)
		{
			var shortFileName = Path.GetFileNameWithoutExtension(fileName);
			var contents = File.ReadAllText(fileName);
			var collections = JsonSerializer.Deserialize<List<TestCollection>>(contents, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			foreach (var collection in collections!)
			{
				collection.IsOptional = fileName.Contains("optional");
				foreach (var test in collection.Tests)
				{
					var optional = collection.IsOptional ? "(optional)/" : null;
					var name = $"{shortFileName}/{optional}{collection.Description.Kebaberize()}/{test.Description.Kebaberize()}";
					var optionsCopy = EvaluationOptions.From(options);
					allTests.Add(new TestCaseData(collection, test, shortFileName, optionsCopy) { TestName = name });
				}
			}
		}

		return allTests;
	}

	static Suite()
	{
		var task = Vocabularies.Register();
		if (task.IsCompleted) return;

		task.RunSynchronously();
	}

	[OneTimeSetUp]
	public static void LoadRemoteSchemas()
	{
		var remotesPath = _useExternal ? _externalRemoteSchemasPath : _remoteSchemasPath;

		var remotesFilePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, remotesPath)
			.AdjustForPlatform();
		if (!Directory.Exists(remotesFilePath))
			throw new Exception("cannot find remotes path");

		DataKeyword.Fetch = async uri =>
		{
			var filePath = uri.OriginalString.Replace("http://localhost:1234", remotesPath);
			var text = await File.ReadAllTextAsync(filePath);
			return JsonNode.Parse(text);
		};
	}

	[TestCaseSource(nameof(TestCases))]
	public async Task Test(TestCollection collection, TestCase test, string fileName, EvaluationOptions options)
	{
		var serializerOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		Console.WriteLine();
		Console.WriteLine();
		Console.WriteLine(fileName);
		Console.WriteLine(collection.Description);
		Console.WriteLine(test.Description);
		if (test.Error)
			Console.WriteLine("expected error");
		else
			Console.WriteLine(test.Valid ? "valid" : "invalid");
		Console.WriteLine();
		Console.WriteLine(JsonSerializer.Serialize(collection.Schema, serializerOptions));
		Console.WriteLine();
		Console.WriteLine(test.Data.AsJsonString());
		Console.WriteLine();

		if (test.Error)
		{
			Assert.ThrowsAsync(Is.InstanceOf<Exception>(), () => collection.Schema.Evaluate(test.Data, options));
			return;
		}

		var result = await collection.Schema.Evaluate(test.Data, options);
		//result.ToBasic();
		Console.WriteLine(JsonSerializer.Serialize(result, serializerOptions));

		if (collection.IsOptional && result.IsValid != test.Valid)
			Assert.Inconclusive("Test optional");
		Assert.AreEqual(test.Valid, result.IsValid);
	}

	public static IEnumerable<TestCaseData> CoreTestCases()
	{
		// ReSharper disable once HeuristicUnreachableCode
		var testCasesPath = _useExternal ? _externalCoreSchemasPath : _coreSchemasPath;

		var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, testCasesPath)
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath)) return Enumerable.Empty<TestCaseData>();

		var fileNames = Directory.GetFiles(testsPath, "*.json");

		var allTests = new List<TestCaseData>();
		foreach (var fileName in fileNames)
		{
			var shortFileName = Path.GetFileNameWithoutExtension(fileName);
			var contents = File.ReadAllText(fileName);

			var name = $"${shortFileName}";
			allTests.Add(new TestCaseData(contents) { TestName = name });
		}

		return allTests;
	}

	[TestCaseSource(nameof(CoreTestCases))]
	public void CoreKeywordsAreInvalid(string schemaText)
	{
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<JsonSchema>(schemaText));
	}

	[Test]
	public void EnsureTestSuiteConfiguredForServerBuild()
	{
		Assert.IsFalse(_useExternal);
	}
}