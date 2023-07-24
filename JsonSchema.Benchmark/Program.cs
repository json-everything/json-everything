using System;
using System.Text.Json.Nodes;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Json.Schema.Benchmark.Suite;

namespace Json.Schema.Benchmark;

class Program
{
	static void Main(string[] args)
	{
#if DEBUG
		IConfig config = new DebugBuildConfig();
		config.WithOptions(ConfigOptions.DisableOptimizationsValidator);
		var summary = BenchmarkRunner.Run<ConstraintsRunner>(config);
#else
		var summary = BenchmarkRunner.Run<ConstraintsRunner>();
#endif
	}
}

[MemoryDiagnoser]
public class ConstraintsRunner
{
	[GlobalSetup]
	public void Setup()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer);

		var instance = 3;

		_ = schema.Evaluate(instance);

		Schema = new JsonSchemaBuilder()
			.AllOf(
				new JsonSchemaBuilder().Minimum(10),
				new JsonSchemaBuilder().Minimum(20)
			)
			.Build();
	}

	[Params(10, 100, 1000)]
	public int RunCount { get; set; }

	public JsonSchema Schema { get; set; }
	public JsonNode Instance { get; set; } = 15;

	[Benchmark]
	public void Test()
	{
		for (int i = 0; i < RunCount; i++)
		{
			_ = Schema.Evaluate2(Instance);
		}
	}

	[Benchmark]
	public void Test_Legacy()
	{
		for (int i = 0; i < RunCount; i++)
		{
			_ = Schema.Evaluate(Instance);
		}
	}
}