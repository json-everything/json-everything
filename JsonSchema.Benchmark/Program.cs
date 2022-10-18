using System;
using BenchmarkDotNet.Running;
using Json.Schema.Benchmark.Suite;

namespace Json.Schema.Benchmark;

class Program
{
	static void Main(string[] args)
	{
		TestSuiteRunner.LoadRemoteSchemas();

		var summary = BenchmarkRunner.Run<TestSuiteRunner>();
	}
}