using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Json.Logic;
using Json.More;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Json.Benchmarks.LogicSuite;

[MemoryDiagnoser]
public class SuiteRunner
{
#if DEBUG
	private const string _benchmarkOffset = @"";
#else
	private const string _benchmarkOffset = @"../../../../";
#endif
	private const string _testCasesPath = @"../../../Files/tests.json";

	private static Test?[] _tests;

	private static Test?[] GetSuite()
	{
		var text = Task.Run(async () =>
		{
			var testsPath = Path.Combine(Directory.GetCurrentDirectory(), _benchmarkOffset, _testCasesPath);

			string? content = null;
			try
			{
				using var client = new HttpClient();
				using var request = new HttpRequestMessage(HttpMethod.Get, "https://jsonlogic.com/tests.json");
				using var response = await client.SendAsync(request);

				content = await response.Content.ReadAsStringAsync();

				await File.WriteAllTextAsync(testsPath, content);
			}
			catch (Exception e)
			{
				content ??= await File.ReadAllTextAsync(testsPath);

				Console.WriteLine(e);
			}
			return content;

		}).Result;

		var testSuite = JsonSerializer.Deserialize<Test?[]>(text)!;

		return testSuite;
	}


	[GlobalSetup]
	public void BenchmarkSetup()
	{
		_tests = GetSuite();
	}

	[Params(1,10,100)]
	public int Count { get; set; }

	[Benchmark]
	public int Models()
	{
		for (int i = 0; i < Count; i++)
		{
			foreach (var test in _tests)
			{
				if (test is null) continue;

				var rule = JsonSerializer.Deserialize<Rule>(test.Logic);

				var actual = rule?.Apply(test.Data);

				_ = test.Expected.IsEquivalentTo(actual);
			}
		}

		return Count;
	}

	[Benchmark]
	public int Nodes()
	{
		for (int i = 0; i < Count; i++)
		{
			foreach (var test in _tests)
			{
				if (test is null) continue;
			
				var rule = JsonNode.Parse(test.Logic);

				var actual = JsonLogic.Apply(rule, test.Data);

				_ = test.Expected.IsEquivalentTo(actual);
			}
		}

		return Count;
	}
}