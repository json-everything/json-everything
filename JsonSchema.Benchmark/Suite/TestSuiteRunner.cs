using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using BenchmarkDotNet.Attributes;
using Json.More;

namespace Json.Schema.Benchmark.Suite;

[MemoryDiagnoser]
public class TestSuiteRunner
{
	private const string _benchmarkOffset = @"../../../../";
	private const string _testCasesPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/tests";
	private const string _remoteSchemasPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/remotes";

	private const bool _runDraftNext = true;
	private const bool _runIsolatedTests = false;
	private const bool _runSingleTest = true;
	private const int _individualTestRunCount = 100;
	private const string _isolatedTestFile = "properties";
	private const string _isolatedDraft = "draft-next";

	private static readonly JsonSchema _schema = new JsonSchemaBuilder()
		.Schema(MetaSchemas.DraftNextId)
		.Type(SchemaValueType.Object)
		.Properties(
			("foo", new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(100)
			),
			("bar", new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.PrefixItems(
					new JsonSchemaBuilder().Type(SchemaValueType.String),
					new JsonSchemaBuilder().Type(SchemaValueType.Boolean)
				)
				.Items(false)
			)
		)
		.Required("foo", "bar")
		.AdditionalProperties(false);

	private List<TestCollection> _testCollections;

	private static IEnumerable<TestCollection> GetAllTests()
	{
		return LoadTests("draft6")
			.Concat(LoadTests("draft7"))
			.Concat(LoadTests("draft2019-09"))
			.Concat(LoadTests("draft2020-12"))
			.Concat(_runDraftNext ? LoadTests("draft-next") : Enumerable.Empty<TestCollection>());
	}

	private static IEnumerable<TestCollection> GetIsolatedTests()
	{
		return LoadTests(_isolatedDraft).Where(t => t.Filename == _isolatedTestFile);
	}

	private static IEnumerable<TestCollection> LoadTests(string draftFolder)
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
				collection.Filename = Path.GetFileNameWithoutExtension(fileName);
				collection.IsOptional = fileName.Contains("optional");
				collection.Options = EvaluationOptions.From(options);

				yield return collection;
			}
		}
	}

	public static IEnumerable<TestCollection> GetSingleTest()
	{
		yield return new TestCollection
		{
			Filename = "local",
			Schema = _schema,
			Tests = new List<TestCase>
			{
				new()
				{
					Description = "valid data",
					Data = new JsonObject
					{
						["foo"] = 15,
						["bar"] = new JsonArray { "string", false }
					},
					Valid = true
				},
				new()
				{
					Description = "missing bar[1] is okay",
					Data = new JsonObject
					{
						["foo"] = 15,
						["bar"] = new JsonArray { "string" }
					},
					Valid = true
				},
				new()
				{
					Description = "valid data",
					Data = new JsonObject
					{
						["foo"] = 15,
						["bar"] = new JsonArray { "string", false }
					},
					Valid = true
				},
				new()
				{
					Description = "foo too high",
					Data = new JsonObject
					{
						["foo"] = 150,
						["bar"] = new JsonArray { "string", false }
					},
					Valid = false
				},
				new()
				{
					Description = "foo too low",
					Data = new JsonObject
					{
						["foo"] = 5,
						["bar"] = new JsonArray { "string", false }
					},
					Valid = false
				},
				new()
				{
					Description = "bar[0] not a string",
					Data = new JsonObject
					{
						["foo"] = 15,
						["bar"] = new JsonArray { 2, false }
					},
					Valid = false
				},
				new()
				{
					Description = "bar[1] not a boolean",
					Data = new JsonObject
					{
						["foo"] = 15,
						["bar"] = new JsonArray { "string", 2 }
					},
					Valid = false
				},
				new()
				{
					Description = "bar has too many items",
					Data = new JsonObject
					{
						["foo"] = 15,
						["bar"] = new JsonArray { "string", false, "disallowed" }
					},
					Valid = false
				},
				new()
				{
					Description = "too many properties",
					Data = new JsonObject
					{
						["foo"] = 15,
						["bar"] = new JsonArray { "string", false },
						["baz"] = true
					},
					Valid = false
				},
				new()
				{
					Description = "missing foo",
					Data = new JsonObject
					{
						["bar"] = new JsonArray { "string", false }
					},
					Valid = false
				},
				new()
				{
					Description = "missing bar",
					Data = new JsonObject
					{
						["foo"] = 15
					},
					Valid = false
				}
			}
		};
	}

	private static IEnumerable<TestCollection> GetTests()
	{
		if (_runSingleTest) return GetSingleTest();
		if (_runIsolatedTests) return GetIsolatedTests();

		return GetAllTests();
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

	[GlobalSetup]
	public void Setup()
	{
		_testCollections = GetTests().ToList();
	}

	[Benchmark]
	public int RunLegacySuiteOnce()
	{
		int i = 0;

		foreach (var collection in _testCollections)
		{
			foreach (var test in collection.Tests)
			{
				BenchmarkLegacy(collection, test, 1);
				i++;
			}
		}

		return i;
	}

	[Benchmark]
	public int RunLegacySuiteManyTimes()
	{
		int i = 0;

		foreach (var collection in _testCollections)
		{
			foreach (var test in collection.Tests)
			{
				BenchmarkLegacy(collection, test, _individualTestRunCount);
				i++;
			}
		}

		return i;
	}

	private static void BenchmarkLegacy(TestCollection collection, TestCase test, int count)
	{
		if (!InstanceIsDeserializable(test.Data)) return;

		for (int i = 0; i < count; i++)
		{
			var _ = collection.Schema.Evaluate(test.Data, collection.Options);
		}
	}

	[Benchmark]
	public int RunFunctionalSuiteOnce()
	{
		int i = 0;

		foreach (var collection in _testCollections)
		{
			foreach (var test in collection.Tests)
			{
				BenchmarkFunctional(collection, test, 1);
				i++;
			}
		}

		return i;
	}

	[Benchmark]
	public int RunFunctionalSuiteManyTimes()
	{
		int i = 0;

		foreach (var collection in _testCollections)
		{
			foreach (var test in collection.Tests)
			{
				BenchmarkFunctional(collection, test, _individualTestRunCount);
				i++;
			}
		}

		return i;
	}

	private static void BenchmarkFunctional(TestCollection collection, TestCase test, int count)
	{
		if (!InstanceIsDeserializable(test.Data)) return;

		try
		{
			for (int i = 0; i < count; i++)
			{
				var _ = collection.Schema.EvaluateCompiled(test.Data, collection.Options);
			}
		}
		catch
		{
			// ignored
		}
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