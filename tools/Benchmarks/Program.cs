using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Json.Benchmarks.LogicSuite;
using Json.Benchmarks.Pointer;
using Json.Benchmarks.SchemaSuite;
using Json.Schema.Benchmark.Functional;

namespace Json.Benchmarks;

class Program
{
	static void Main(string[] args)
	{
#if DEBUG
		IConfig config = new DebugBuildConfig();
		config.WithOptions(ConfigOptions.DisableOptimizationsValidator);
		var summary = BenchmarkRunner.Run<FunctionalRunner>(config);

		var runner = new SuiteRunner();
		runner.BenchmarkSetup();
		runner.Models();
#else
		var summary = BenchmarkRunner.Run<TestSuiteRunner>();
#endif
	}
}