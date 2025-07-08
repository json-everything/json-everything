using BenchmarkDotNet.Running;
using Json.LegacyBenchmarks;
using System;

namespace Json.LegacyBenchmarks;

class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("Running JSON Schema validation benchmarks with JsonSchema.Net v7.3.4...");

#if DEBUG
		var runner = new ValidationRunner();
		runner.Count = 1;
		runner.LegacySchemaValidation();
		Console.WriteLine("Debug run completed successfully.");
#else
		BenchmarkRunner.Run<ValidationRunner>();
#endif
	}
} 