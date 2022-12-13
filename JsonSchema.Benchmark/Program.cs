using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Json.Schema.Benchmark.Suite;

namespace Json.Schema.Benchmark;

class Program
{
	static void Main(string[] args)
	{
		TestSuiteRunner.LoadRemoteSchemas();

#if DEBUG
		IConfig config = new DebugBuildConfig();
		config.WithOptions(ConfigOptions.DisableOptimizationsValidator);
		var summary = BenchmarkRunner.Run<TestSuiteRunner>(config);
#else
		var summary = BenchmarkRunner.Run<TestSuiteRunner>();
#endif
	}
}