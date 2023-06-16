using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.Tests.Suite;

public class Output
{
	private const string _testCasesPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/output-tests";

	private const bool _useExternal = false;
	private const bool _runDraftNext = true;
	private const string _externalTestCasesPath = @"../../../../../JSON-Schema-Test-Suite/output-tests";

	private static readonly JsonConverter<EvaluationResults> _resultsConverter = new EvaluationResultsJsonConverter();
	private static readonly JsonConverter<EvaluationResults> _legacyResultsConverter = new Pre202012EvaluationResultsJsonConverter();

	private static readonly SpecVersion[] _unsupportedVersions =
	{
		SpecVersion.Draft201909,
		SpecVersion.Draft202012
	};

	public static IEnumerable<TestCaseData> TestCases()
	{
		return GetTests("draft2019-09")
			.Concat(GetTests("draft2020-12"))
			.Concat(_runDraftNext ? GetTests("draft-next") : Enumerable.Empty<TestCaseData>());
	}

	private static IEnumerable<TestCaseData> GetTests(string draftFolder)
	{
		// ReSharper disable once HeuristicUnreachableCode
		var testCasesPath = _useExternal ? _externalTestCasesPath : _testCasesPath;

		var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, testCasesPath, $"{draftFolder}/")
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath)) return Enumerable.Empty<TestCaseData>();

		var fileNames = Directory.GetFiles(testsPath, "*.json", SearchOption.AllDirectories);
		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};
		switch (draftFolder)
		{
			case "draft2019-09":
				options.EvaluateAs = SpecVersion.Draft201909;
				break;
			case "draft2020-12":
				options.EvaluateAs = SpecVersion.Draft202012;
				break;
			case "draft-next":
				// options.ValidateAs = Draft.DraftNext;
				break;
		}

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
			var collections = JsonSerializer.Deserialize<List<TestCollection>>(contents, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

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
	public async Task Test(TestCollection collection, TestCase test, string format, string fileName, EvaluationOptions options)
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
		Console.WriteLine();
		Console.WriteLine(JsonSerializer.Serialize(collection.Schema, serializerOptions));
		Console.WriteLine();
		Console.WriteLine(test.Data.AsJsonString());
		Console.WriteLine();

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
		var result = await collection.Schema.Evaluate(test.Data, options);
		var serializedResult = JsonSerializer.SerializeToNode(result, new JsonSerializerOptions
		{
			Converters = { converter }
		});
		Console.WriteLine(JsonSerializer.Serialize(serializedResult, serializerOptions));
		Console.WriteLine();


		var outputSchema = test.Output![format];
		result = await outputSchema.Evaluate(serializedResult, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		});

		if (_unsupportedVersions.Contains(options.EvaluateAs))
		{
			Console.WriteLine(JsonSerializer.Serialize(result, serializerOptions));

			if (!result.IsValid)
				Assert.Inconclusive("not fully supported");

			return;
		}

		result.AssertValid();
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
			Console.WriteLine(e.Message);
			return false;
		}
	}

	[Test]
	public void EnsureTestSuiteConfiguredForServerBuild()
	{
		Assert.IsFalse(_useExternal);
		//Assert.IsFalse(_runDraftNext);
	}
}