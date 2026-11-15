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
[SimpleJob(RuntimeMoniker.Net11_0)]
public class TestSuiteRunner
{
	private const string _benchmarkOffset = @"../../../../../";
	private const string _testCasesPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/validation/tests";

	private const bool _runDraftNext = true;

	private static readonly JsonSerializerOptions _serializerOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	private static readonly string[] _supportedProposals =
	[
		//"propertyDependencies"
	];

	public static IEnumerable<TestCollection> GetAllTests()
	{
		return GetTests(SuiteDialect.Draft06)
			.Concat(GetTests(SuiteDialect.Draft07))
			.Concat(GetTests(SuiteDialect.Draft201909))
			.Concat(GetTests(SuiteDialect.Draft202012))
			.Concat(_runDraftNext ? GetTests(SuiteDialect.V1) : []);
	}

	private static IEnumerable<TestCollection> GetTests(SuiteDialect suiteDialect)
	{
		var testsPath = Path.Combine(Directory.GetCurrentDirectory(), _benchmarkOffset, _testCasesPath)
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath)) yield break;

		var fileNames = Directory.GetFiles(testsPath, "*.json", SearchOption.AllDirectories);

		foreach (var fileName in fileNames)
		{
			var filePath = fileName.Split('/', '\\');
			var proposals = Array.IndexOf(filePath, "proposals");
			if (proposals != -1 && proposals + 1 < filePath.Length)
			{
				var proposal = filePath[proposals + 1];
				if (!_supportedProposals.Contains(proposal)) continue;
			}

			var contents = File.ReadAllText(fileName);
			var testFile = JsonSerializer.Deserialize<TestFile>(contents, _serializerOptions);

			foreach (var collection in testFile!.Tests)
			{
				if (!suiteDialect.IsCompatible(collection.Compatibility)) continue;
				if (!suiteDialect.SupportsTags(collection)) continue;

				collection.IsOptional = fileName.Contains("optional");
				collection.Dialect = suiteDialect.Dialect;
				collection.EvaluationOptions = new EvaluationOptions
				{
					OutputFormat = OutputFormat.Hierarchical,
					RequireFormatValidation = SuiteDialect.RequiresFormatValidation(collection)
				};

				yield return collection;
			}
		}
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
				try
				{
					for (int j = 0; j < n; j++)
					{
						var schema = JsonSchema.Build(collection.Schema, collection.CreateBuildOptions());
						_ = schema.Evaluate(test.Data, collection.EvaluationOptions);
					}
				}
				catch when (collection.IsOptional)
				{
					// optional tests may cover functionality that is explicitly unsupported
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
				try
				{
					var schema = JsonSchema.Build(collection.Schema, collection.CreateBuildOptions());
					for (int j = 0; j < n; j++)
					{
						_ = schema.Evaluate(test.Data, collection.EvaluationOptions);
					}
				}
				catch when (collection.IsOptional)
				{
					// optional tests may cover functionality that is explicitly unsupported
				}
				i++;
			}
		}

		return i;
	}
}
