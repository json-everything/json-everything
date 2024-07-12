//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text.Json;
//using System.Text.Json.Nodes;
//using System.Text.RegularExpressions;
//using BenchmarkDotNet.Attributes;
//using Json.More;

//using ExperimentsSchema = Json.Schema.Experiments.JsonSchema;
//using ExperimentsOptions = Json.Schema.Experiments.EvaluationOptions;
//using ExperimentsResults = Json.Schema.Experiments.EvaluationResults;

//namespace Json.Schema.Benchmark.Suite;

//[MemoryDiagnoser]
//public class ExperimentalSuiteRunner
//{
//	private const string _benchmarkOffset = @"../../../../";
//	private const string _testCasesPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/tests";
//	private const string _remoteSchemasPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/remotes";

//	private const bool _runDraftNext = true;
//	private static (ExperimentalTestCollection, TestCase)[] _allTests;

//	public static IEnumerable<ExperimentalTestCollection> GetAllTests()
//	{
//		return GetTests("draft6")
//			.Concat(GetTests("draft7"))
//			.Concat(GetTests("draft2019-09"))
//			.Concat(GetTests("draft2020-12"))
//			.Concat(_runDraftNext ? GetTests("draft-next") : Enumerable.Empty<ExperimentalTestCollection>());
//	}

//	private static IEnumerable<ExperimentalTestCollection> GetTests(string draftFolder)
//	{
//		// ReSharper disable once HeuristicUnreachableCode

//		var testsPath = Path.Combine(Directory.GetCurrentDirectory(), _benchmarkOffset, _testCasesPath, $"{draftFolder}/")
//			.AdjustForPlatform();
//		if (!Directory.Exists(testsPath))
//		{
//			Console.WriteLine("Cannot find directory: " + testsPath);
//			throw new DirectoryNotFoundException(testsPath);
//		}

//		var fileNames = Directory.GetFiles(testsPath, "*.json", SearchOption.AllDirectories);

//		foreach (var fileName in fileNames)
//		{
//			var shortFileName = Path.GetFileNameWithoutExtension(fileName);

//			// adjust for format
//			var options = new ExperimentsOptions();
//			options.DefaultMetaSchema = draftFolder switch
//			{
//				"draft6" => MetaSchemas.Draft6Id,
//				"draft7" => MetaSchemas.Draft7Id,
//				"draft2019-09" => MetaSchemas.Draft201909Id,
//				"draft2020-12" => MetaSchemas.Draft202012Id,
//				"draft-next" => MetaSchemas.DraftNextId,
//				_ => options.DefaultMetaSchema
//			};
//			options.RequireFormatValidation = fileName.Contains("format/".AdjustForPlatform()) &&
//			                                  // uri-template will throw an exception as it's explicitly unsupported
//			                                  shortFileName != "uri-template";


//			var contents = File.ReadAllText(fileName);
//			var collections = JsonSerializer.Deserialize<List<ExperimentalTestCollection>>(contents, new JsonSerializerOptions
//			{
//				PropertyNameCaseInsensitive = true
//			});

//			foreach (var collection in collections!)
//			{
//				collection.IsOptional = fileName.Contains("optional");
//				collection.Options = options;

//				yield return collection;
//			}
//		}
//	}

//	private static void LoadRemoteSchemas()
//	{
//		// ReSharper disable once HeuristicUnreachableCode
//		var remotesPath = Path.Combine(Directory.GetCurrentDirectory(), _benchmarkOffset, _remoteSchemasPath)
//			.AdjustForPlatform();
//		if (!Directory.Exists(remotesPath)) throw new Exception("Cannot find remotes folder");

//		var fileNames = Directory.GetFiles(remotesPath, "*.json", SearchOption.AllDirectories);

//		foreach (var fileName in fileNames)
//		{
//			var schema = (JsonObject)JsonNode.Parse(File.ReadAllText(fileName))!;
//			var uri = new Uri(fileName.Replace(remotesPath, "http://localhost:1234").Replace('\\', '/'));
//			Experiments.SchemaRegistry.Global.Register(uri, schema);
//		}
//	}

//	[GlobalSetup]
//	public void BenchmarkSetup()
//	{
//		LoadRemoteSchemas();
//		_allTests = GetAllTests().SelectMany(x => x.Tests.Where(t => InstanceIsDeserializable(t.Data)).Select(t => (x, t))).ToArray();
//	}

//	[Benchmark]
//	[Arguments(1)]
//	[Arguments(10)]
//	public int RunSuite(int n)
//	{
//		int i = 0;

//		foreach (var (collection, test) in _allTests)
//		{
//			Benchmark(collection, test, n);
//			i++;
//		}

//		return i;
//	}

//	private void Benchmark(ExperimentalTestCollection collection, TestCase test, int n)
//	{
//		if (!InstanceIsDeserializable(test.Data)) return;

//		for (int i = 0; i < n; i++)
//		{
//			try
//			{
//				_ = ExperimentsSchema.Evaluate(collection.Schema, test.Data, collection.Options);
//			}
//			catch (RegexParseException)
//			{
//			}
//		}
//	}

//	private static bool InstanceIsDeserializable(in JsonNode? testData)
//	{
//		try
//		{
//			var value = (testData as JsonValue)?.GetValue<object>();
//			if (value is null) return true;
//			if (value is string) return false;
//			if (value is JsonElement { ValueKind: JsonValueKind.Number } element)
//				return element.TryGetDecimal(out _);
//			if (value.GetType().IsNumber())
//			{
//				// some tests involve numbers larger than c# can handle.  fortunately, they're optional.
//				return true;
//			}

//			return true;
//		}
//		catch
//		{
//			return false;
//		}
//	}
//}

//[MemoryDiagnoser]
//public class BothRunner
//{
//	private readonly TestSuiteRunner _testSuiteRunner = new();
//	private readonly ExperimentalSuiteRunner _experimentalSuiteRunner = new();

//	[GlobalSetup]
//	public void BenchmarkSetup()
//	{
//		_testSuiteRunner.BenchmarkSetup();
//		_experimentalSuiteRunner.BenchmarkSetup();
//	}

//	[Benchmark]
//	[Arguments(1)]
//	[Arguments(10)]
//	[Arguments(50)]
//	public int Legacy(int n)
//	{
//		return _testSuiteRunner.RunSuite(n);
//	}

//	[Benchmark]
//	[Arguments(1)]
//	[Arguments(10)]
//	[Arguments(50)]
//	public int Experimental(int n)
//	{
//		return _experimentalSuiteRunner.RunSuite(n);
//	}

//}