using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Json.Schema.Benchmark.Suite;

namespace Json.Schema.Benchmark;

class Program
{
	static void Main(string[] args)
	{
#if DEBUG
		var runner = new TestSuiteRunner();
		runner.BenchmarkSetup();
		runner.Legacy(1);
#else
		var summary = BenchmarkRunner.Run<TestSuiteRunner>();
#endif
	}
}