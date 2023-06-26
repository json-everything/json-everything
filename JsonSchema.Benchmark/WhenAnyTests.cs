using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Json.Schema.Benchmark;

[MemoryDiagnoser]
public class WhenAnyTests
{
	[Benchmark]
	[Arguments(true)]
	[Arguments(false)]
	public async Task<int> PredicatedWhenAny(bool sameTimings)
	{
		var tasks = Enumerable.Range(1, 100)
			.Select(async x =>
			{
				await Task.Delay(sameTimings ? 100 : x * 100);
				return x;
			});

		await tasks.WhenAny(x => x == 30);

		return 0;
	}

	[Benchmark]
	[Arguments(true)]
	[Arguments(false)]
	public async Task<int> WhenAll(bool sameTimings)
	{
		var tasks = Enumerable.Range(1, 100)
			.Select(async x =>
			{
				await Task.Delay(sameTimings ? 100 : x * 100);
				return x;
			});

		await Task.WhenAll(tasks);

		return 0;
	}
}