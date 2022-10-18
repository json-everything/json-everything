using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using BenchmarkDotNet.Attributes;
using Json.More;

namespace Json.Schema.Benchmark.Suite;

[MemoryDiagnoser(false)]
public class TestSuiteRunner
{
	private const string _benchmarkOffset = @"../../../../";
	private const string _testCasesPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/tests";
	private const string _remoteSchemasPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/remotes";

	private const bool _runDraftNext = true;

	public static IEnumerable<TestCollection> GetAllTests()
	{
		return GetTests("draft6")
			.Concat(GetTests("draft7"))
			.Concat(GetTests("draft2019-09"))
			.Concat(GetTests("draft2020-12"))
			.Concat(_runDraftNext ? GetTests("draft-next") : Enumerable.Empty<TestCollection>());
	}

	private static IEnumerable<TestCollection> GetTests(string draftFolder)
	{
		// ReSharper disable once HeuristicUnreachableCode

		var testsPath = Path.Combine(Directory.GetCurrentDirectory(), _benchmarkOffset, _testCasesPath, $"{draftFolder}/")
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath))
		{
			Console.WriteLine("Cannot find directory: " + testsPath);
			throw new DirectoryNotFoundException(testsPath);
		}

		var fileNames = Directory.GetFiles(testsPath, "*.json", SearchOption.AllDirectories);
		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Basic
		};
		switch (draftFolder)
		{
			case "draft6":
				options.EvaluateAs = Draft.Draft6;
				break;
			case "draft7":
				options.EvaluateAs = Draft.Draft7;
				break;
			case "draft2019-09":
				options.EvaluateAs = Draft.Draft201909;
				break;
			case "draft2020-12":
				options.EvaluateAs = Draft.Draft202012;
				break;
			case "draft-next":
				// options.ValidateAs = Draft.DraftNext;
				break;
		}

		foreach (var fileName in fileNames)
		{
			var shortFileName = Path.GetFileNameWithoutExtension(fileName);

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
				collection.Options = EvaluationOptions.From(options);

				yield return collection;
			}
		}
	}

	public static void LoadRemoteSchemas()
	{
		// ReSharper disable once HeuristicUnreachableCode
		var remotesPath = Path.Combine(Directory.GetCurrentDirectory(), _remoteSchemasPath)
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

	[Benchmark]
	public int RunSuite()
	{
		int i = 0;
		var collections = GetAllTests();

		foreach (var collection in collections)
		{
			foreach (var test in collection.Tests)
			{
				Benchmark(collection, test);
				i++;
			}
		}

		return i;
	}

	private static void Benchmark(TestCollection collection, TestCase test)
	{
		if (!InstanceIsDeserializable(test.Data)) return;

		var result = collection.Schema.Evaluate(test.Data, collection.Options);
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
}