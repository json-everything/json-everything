using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Json.Schema.Benchmark.Suite;

namespace Json.Schema.Benchmark;

class Program
{
	static async Task Main(string[] args)
	{
		await TestSuiteRunner.LoadRemoteSchemas();

#if DEBUG
		IConfig config = new DebugBuildConfig();
		config.WithOptions(ConfigOptions.DisableOptimizationsValidator);
		var summary = BenchmarkRunner.Run<TestSuiteRunner>(config);
#else
		var summary = BenchmarkRunner.Run<TestSuiteRunner>();
#endif
	}
}