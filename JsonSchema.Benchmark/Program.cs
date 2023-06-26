using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Json.Schema.Benchmark.Suite;

namespace Json.Schema.Benchmark;

class Program
{
	static void Main(string[] args)
	{
		IConfig config = new DebugBuildConfig();
#if DEBUG
		config.WithOptions(ConfigOptions.DisableOptimizationsValidator);
#endif
		_ = BenchmarkRunner.Run<TestSuiteRunner>(config);
	}
}