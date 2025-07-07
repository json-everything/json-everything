﻿using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Json.Benchmarks.LogicSuite;
using Json.Benchmarks.Pointer;
using Json.Benchmarks.Schema;
//using Json.Benchmarks.SchemaSuite;

namespace Json.Benchmarks;

class Program
{
	static void Main(string[] args)
	{
#if DEBUG
		//IConfig config = new DebugBuildConfig();
		//config.WithOptions(ConfigOptions.DisableOptimizationsValidator);
		//var summary = BenchmarkRunner.Run<TestSuiteRunner>(config);

		var runner = new ValidationRunner();
		runner.ValidateWithSchemaCompilation();
		
#else
		var summary = BenchmarkRunner.Run<ValidationRunner>();
#endif
	}
}