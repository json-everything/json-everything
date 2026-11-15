using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests.Suite;

public class Output
{
	private const string _testCasesPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/output-tests";

	private const bool _useExternal = false;
	private const bool _runDraftNext = true;
	private const string _externalTestCasesPath = @"../../../../../JSON-Schema-Test-Suite/output-tests";

	private static readonly JsonConverter<EvaluationResults> _resultsConverter = new EvaluationResultsJsonConverter();
#pragma warning disable CS0618 // Type or member is obsolete
	private static readonly JsonConverter<EvaluationResults> _legacyResultsConverter = new Pre202012EvaluationResultsJsonConverter();
#pragma warning restore CS0618 // Type or member is obsolete

	private static readonly SpecVersion[] _unsupportedVersions =
	[
		SpecVersion.Draft201909,
		SpecVersion.Draft202012
	];

	public static IEnumerable<TestCaseData> TestCases()
	{
		return GetTests("draft2019-09")
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
			if (shortFileName == "output-schema")
			{
				var outputSchema = JsonSchema.FromFile(fileName);
				SchemaRegistry.Global.Register(new Uri(fileName), outputSchema);
				continue;
			}

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
					foreach (var format in test.Output!.Keys)
					{
						var optional = collection.IsOptional ? "(optional) / " : null;
						var name = $"{draftFolder} / {shortFileName} / {optional}{collection.Description} / {test.Description} / {format}";
						var optionsCopy = EvaluationOptions.From(options);
						allTests.Add(new TestCaseData(collection, test, format, shortFileName, optionsCopy) { TestName = name });
					}
				}
			}
		}

		return allTests;
	}

	[TestCaseSource(nameof(TestCases))]
	public void Test(TestCollection collection, TestCase test, string format, string fileName, EvaluationOptions options)
	{
		TestConsole.WriteLine();
		TestConsole.WriteLine();
		TestConsole.WriteLine(fileName);
		TestConsole.WriteLine(collection.Description);
		TestConsole.WriteLine(test.Description);
		TestConsole.WriteLine();
		TestConsole.WriteLine(JsonSerializer.Serialize(collection.Schema, TestEnvironment.TestOutputSerializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(test.Data.AsJsonString());
		TestConsole.WriteLine();

		if (!InstanceIsDeserializable(test.Data))
			Assert.Inconclusive("Instance not deserializable");

		var (outputFormat, converter) = format switch
		{
			"flag" => (OutputFormat.List, _resultsConverter),
			"basic" => (OutputFormat.List, _legacyResultsConverter),
			"list" => (OutputFormat.List, _resultsConverter),
			"detailed" => ((OutputFormat)(-1), null!),
			"verbose" => (OutputFormat.Hierarchical, _legacyResultsConverter),
			"hierarchical" => (OutputFormat.Hierarchical, _resultsConverter),
			_ => throw new ArgumentOutOfRangeException(nameof(format))
		};
		options.OutputFormat = outputFormat;
		var result = collection.Schema.Evaluate(test.Data, options);
		var optionsWithConverters = new JsonSerializerOptions
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			Converters = { converter }
		};
		var serializedResult = JsonSerializer.SerializeToNode(result, optionsWithConverters);
		TestConsole.WriteLine(JsonSerializer.Serialize(serializedResult, TestEnvironment.TestOutputSerializerOptions));
		TestConsole.WriteLine();


		var outputSchema = test.Output![format];
		result = outputSchema.Evaluate(serializedResult, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		});

		if (_unsupportedVersions.Contains(options.EvaluateAs))
		{
			if (!result.IsValid)
			{
				TestConsole.WriteLine(JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions));
				Assert.Inconclusive("not fully supported");
			}

			return;
		}

		result.AssertValid(hideIfPassed: true);
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