using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Json.Benchmarks.LogicSuite;
using Json.Benchmarks.Pointer;
using Json.Benchmarks.SchemaSuite;
using Json.Schema;

namespace Json.Benchmarks;

class Program
{
	static void Main(string[] args)
	{
#if DEBUG
		//IConfig config = new DebugBuildConfig();
		//config.WithOptions(ConfigOptions.DisableOptimizationsValidator);
		//var summary = BenchmarkRunner.Run<TestSuiteRunner>(config);

		//var runner = new SingleSchemaRunner();
		//while (true)
		//{
		//	runner.BuildOnce(1000);
		//}

		var stopwatch = new Stopwatch();

		stopwatch.Start();
		var schema = JsonSchema.FromFile(@"C:\Users\gregs\Downloads\schema_rand.json");
		stopwatch.Stop();
		Console.WriteLine($"Schema loaded: {stopwatch.ElapsedMilliseconds}ms");

		stopwatch.Reset();
		stopwatch.Start();
		var instance = JsonDocument.Parse(File.ReadAllText(@"C:\Users\gregs\Downloads\schema_data_rand.json")).RootElement;
		stopwatch.Stop();
		Console.WriteLine($"Instance loaded: {stopwatch.ElapsedMilliseconds}ms");

		stopwatch.Reset();
		stopwatch.Start();
		var result = schema.Evaluate(instance);
		stopwatch.Stop();
		Console.WriteLine($"Evaluation complete: {result.IsValid}, {stopwatch.ElapsedMilliseconds/1000.0}s");
		
		Console.ReadLine();
#else
		var summary = BenchmarkRunner.Run<SingleSchemaRunner>();
#endif
	}
}