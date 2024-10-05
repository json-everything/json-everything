using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
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
			.Concat(_runDraftNext ? GetTests("draft-next") : []);
	}

	private static IEnumerable<TestCaseData> GetTests(string draftFolder)
	{
		// ReSharper disable once HeuristicUnreachableCode
		var testCasesPath = _useExternal ? _externalTestCasesPath : _testCasesPath;

		var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, testCasesPath, $"{draftFolder}/")
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath)) return [];

		var fileNames = Directory.GetFiles(testsPath, "*.json", SearchOption.AllDirectories);
		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			EvaluateAs = draftFolder switch
			{
				"draft6" => SpecVersion.Draft6,
				"draft7" => SpecVersion.Draft7,
				"draft2019-09" => SpecVersion.Draft201909,
				"draft2020-12" => SpecVersion.Draft202012,
				"draft-next" => SpecVersion.DraftNext,
				_ => SpecVersion.Unspecified
			}
		};

		var allTests = new List<TestCaseData>();
		foreach (var fileName in fileNames)
		{
			var shortFileName = Path.GetFileNameWithoutExtension(fileName);

			// adjust for format
			options.RequireFormatValidation = fileName.Contains("format/".AdjustForPlatform()) &&
											  // uri-template will throw an exception as it's explicitly unsupported
											  shortFileName != "uri-template";

			var contents = File.ReadAllText(fileName);
			var collections = JsonSerializer.Deserialize<List<TestCollection>>(contents, TestEnvironment.TestSuiteSerializationOptions);

			foreach (var collection in collections!)
			{
				collection.IsOptional = fileName.Contains("optional");
				foreach (var test in collection.Tests)
				{
					var optional = collection.IsOptional ? "(optional) / " : null;
					var name = $"{draftFolder} / {shortFileName} / {optional}{collection.Description} / {test.Description}";
					var optionsCopy = EvaluationOptions.From(options);
					allTests.Add(new TestCaseData(collection, test, shortFileName, optionsCopy) { TestName = name });
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
			var schema = JsonSchema.FromFile(fileName);
			var uri = new Uri(fileName.Replace(remotesPath, "http://localhost:1234").Replace('\\', '/'));
			SchemaRegistry.Global.Register(uri, schema);
		}
	}

	[TestCaseSource(nameof(TestCases))]
	public void Test(TestCollection collection, TestCase test, string fileName, EvaluationOptions options)
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
		TestConsole.WriteLine(test.Data.AsJsonString());
		TestConsole.WriteLine();

		if (!InstanceIsDeserializable(test.Data))
			Assert.Inconclusive("Instance not deserializable");

		var result = collection.Schema.Evaluate(test.Data, options);
		//result.ToBasic();
		TestConsole.WriteLine(JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions));

		if (collection.IsOptional && result.IsValid != test.Valid)
			Assert.Inconclusive("Test optional");
		Assert.That(result.IsValid, Is.EqualTo(test.Valid));
	}

	private static bool InstanceIsDeserializable(in JsonNode? testData)
	{
		try
		{
			var value = (testData as JsonValue)?.GetValue<object>();
			if (value is null) return true;
			if (value is string) return false;
			if (value is JsonElement { ValueKind: JsonValueKind.Number } element)
				return element.TryGetDecimal(out _);
			if (value.GetType().IsNumber())
			{
				// some tests involve numbers larger than c# can handle.  fortunately, they're optional.
				return true;
			}

			return true;
		}
		catch (Exception e)
		{
			TestConsole.WriteLine(e.Message);
			return false;
		}
	}

	[Test]
	public void EnsureTestSuiteConfiguredForServerBuild()
	{
		Assert.That(_useExternal, Is.False);
		//Assert.IsFalse(_runDraftNext);
	}
}