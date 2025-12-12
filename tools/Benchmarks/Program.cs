using System;
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

		var schema = JsonSchema.FromFile(@"C:\Users\gregs\Downloads\schema_rand.json");
		Console.WriteLine("Schema loaded");
		Console.ReadLine();
		var instance = JsonDocument.Parse(File.ReadAllText(@"C:\Users\gregs\Downloads\schema_data_rand.json")).RootElement;
		Console.WriteLine("Instance loaded");
		Console.ReadLine();

		var result = schema.Evaluate(instance);

		Console.WriteLine("Evaluation complete");
		Console.ReadLine();
#else
		var summary = BenchmarkRunner.Run<SingleSchemaRunner>();
#endif
	}
}