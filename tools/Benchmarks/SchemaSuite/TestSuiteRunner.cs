using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Json.Schema;

namespace Json.Benchmarks.SchemaSuite;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, baseline:true)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class TestSuiteRunner
{
	private const string _benchmarkOffset = @"../../../../../";
	private const string _testCasesPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/tests";
	private const string _remoteSchemasPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/remotes";

	private const bool _runDraftNext = true;

	public static IEnumerable<TestCollection> GetAllTests()
	{
		return GetTests("draft6")
			.Concat(GetTests("draft7"))
			.Concat(GetTests("draft2019-09"))
			.Concat(GetTests("draft2020-12"))
			.Concat(_runDraftNext ? GetTests("draft-next") : []);
	}

	private static IEnumerable<TestCollection> GetTests(string draftFolder)
	{
		// ReSharper disable once HeuristicUnreachableCode
		var testsPath = Path.Combine(Directory.GetCurrentDirectory(), _benchmarkOffset, $"{draftFolder}/")
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath)) yield break;

		var fileNames = Directory.GetFiles(testsPath, "*.json", SearchOption.AllDirectories);
		var evaluationOptions = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
		};

		foreach (var fileName in fileNames)
		{
			var shortFileName = Path.GetFileNameWithoutExtension(fileName);

			// adjust for format
			evaluationOptions.RequireFormatValidation = fileName.Contains("format/".AdjustForPlatform()) &&
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
				var keywords = draftFolder switch
				{
					"draft6" => SchemaKeywordRegistry.Draft06,
					"draft7" => SchemaKeywordRegistry.Draft07,
					"draft2019-09" => SchemaKeywordRegistry.Draft201909,
					"draft2020-12" => SchemaKeywordRegistry.Draft202012,
					"draft-next" => SchemaKeywordRegistry.V1,
					_ => throw new ArgumentOutOfRangeException(nameof(draftFolder), $"{draftFolder} is unsupported")
				};

				if (fileName.Contains("format/".AdjustForPlatform()) &&
					// uri-template will throw an exception as it's explicitly unsupported
					shortFileName != "uri-template")
					keywords = keywords.UseFormatValidation();

				collection.BuildOptions = new BuildOptions
				{
					KeywordRegistry = keywords,
					SchemaRegistry = new()
				};
				collection.EvaluationOptions = evaluationOptions;

				yield return collection;
			}
		}
	}

	private static void LoadRemoteSchemas()
	{
		// ReSharper disable once HeuristicUnreachableCode
		var remotesPath = Path.Combine(Directory.GetCurrentDirectory(), _benchmarkOffset, _remoteSchemasPath)
			.AdjustForPlatform();
		if (!Directory.Exists(remotesPath)) throw new Exception("Cannot find remotes folder");

		var fileNames = Directory.GetFiles(remotesPath, "*.json", SearchOption.AllDirectories);

		foreach (var fileName in fileNames)
		{
			try
			{
				var schema = JsonSchema.FromFile(fileName);
				var uri = new Uri(fileName.Replace(remotesPath, "http://localhost:1234").Replace('\\', '/'));
				SchemaRegistry.Global.Register(uri, schema);
			}
			catch (JsonSchemaException)
			{
			}
		}
	}

	[GlobalSetup]
	public void BenchmarkSetup()
	{
		LoadRemoteSchemas();
		_ = GetAllTests();
	}

	[Benchmark]
	[Arguments(1)]
	[Arguments(5)]
	[Arguments(10)]
	[Arguments(50)]
	public int BuildAlways(int n)
	{
		int i = 0;
		var collections = GetAllTests();

		foreach (var collection in collections)
		{
			foreach (var test in collection.Tests)
			{
				for (int j = 0; j < n; j++)
				{
					var schema = JsonSchema.Build(collection.Schema, collection.BuildOptions);
					_ = schema.Evaluate(test.Data, collection.EvaluationOptions);
				}
				i++;
			}
		}

		return i;
	}

	[Benchmark]
	[Arguments(1)]
	[Arguments(5)]
	[Arguments(10)]
	[Arguments(50)]
	public int BuildOnce(int n)
	{
		int i = 0;
		var collections = GetAllTests();

		foreach (var collection in collections)
		{
			foreach (var test in collection.Tests)
			{
				var schema = JsonSchema.Build(collection.Schema, collection.BuildOptions);
				for (int j = 0; j < n; j++)
				{
					_ = schema.Evaluate(test.Data, collection.EvaluationOptions);
				}
				i++;
			}
		}

		return i;
	}
}