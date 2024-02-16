using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using BenchmarkDotNet.Attributes;
using Json.More;

namespace Json.Schema.Benchmark.Suite;

[MemoryDiagnoser]
public class TestSuiteRunner
{
	private const string _benchmarkOffset = @"../../../../";
	private const string _remoteSchemasPath = @"../../../../ref-repos/JSON-Schema-Test-Suite/remotes";
	
	private static void LoadRemoteSchemas()
	{
		// ReSharper disable once HeuristicUnreachableCode
		var remotesPath = Path.Combine(Directory.GetCurrentDirectory(), _benchmarkOffset, _remoteSchemasPath)
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
	public void BenchmarkSetup()
	{
		LoadRemoteSchemas();
		_ = TestSetup<TestCollection>.GetAllTests();
		_ = TestSetup<TestCollectionFunctional>.GetAllTests();
	}

	[Benchmark]
	[Arguments(1)]
	public int ObjectOriented(int n)
	{
		int i = 0;
		var collections = TestSetup<TestCollection>.GetAllTests();

		foreach (var collection in collections)
		{
			foreach (var test in collection.Tests)
			{
				BenchmarkObjectOriented(collection, test, n);
				i++;
			}
		}

		return i;
	}

	[Benchmark]
	[Arguments(1)]
	public int Functional(int n)
	{
		int i = 0;
		var collections = TestSetup<TestCollectionFunctional>.GetAllTests();

		foreach (var collection in collections)
		{
			foreach (var test in collection.Tests)
			{
				BenchmarkFunctional(collection, test, n);
				i++;
			}
		}

		return i;
	}

	private static void BenchmarkObjectOriented(TestCollection collection, TestCase test, int n)
	{
		if (!InstanceIsDeserializable(test.Data)) return;

		for (int i = 0; i < n; i++)
		{
			_ = collection.Schema.Evaluate(test.Data, collection.Options);
		}
	}


	private static void BenchmarkFunctional(TestCollectionFunctional collection, TestCase test, int n)
	{
		if (!InstanceIsDeserializable(test.Data)) return;

		for (int i = 0; i < n; i++)
		{
			_ = JsonSchema.Evaluate(collection.Schema, test.Data);
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