using System;
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