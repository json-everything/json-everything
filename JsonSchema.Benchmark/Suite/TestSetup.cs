using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Json.Schema.Benchmark.Suite;

public static class TestSetup<T>
	where T : ITestCollection
{
	private const string _benchmarkOffset = @"../../../../";
	private const string _testCasesPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/tests";

	private static IEnumerable<T>? _tests;

	public static IEnumerable<T> GetAllTests()
	{
		return _tests ??= GetTests("draft6")
			.Concat(GetTests("draft7"))
			.Concat(GetTests("draft2019-09"))
			.Concat(GetTests("draft2020-12"))
			.Concat(GetTests("draft-next"));
	}

	private static IEnumerable<T> GetTests(string draftFolder)
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
			OutputFormat = OutputFormat.List
		};
		switch (draftFolder)
		{
			case "draft6":
				options.EvaluateAs = SpecVersion.Draft6;
				break;
			case "draft7":
				options.EvaluateAs = SpecVersion.Draft7;
				break;
			case "draft2019-09":
				options.EvaluateAs = SpecVersion.Draft201909;
				break;
			case "draft2020-12":
				options.EvaluateAs = SpecVersion.Draft202012;
				break;
			case "draft-next":
				// options.ValidateAs = SpecVersion.DraftNext;
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
			var collections = JsonSerializer.Deserialize<List<T>>(contents, new JsonSerializerOptions
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
}