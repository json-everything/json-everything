using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public async Task Test()
	{
		var rand = new Random();
		var cancellationToken = new CancellationTokenSource();
		var tasks = Enumerable.Range(0, 10).Select(x => Task.Run(async () => 
		{
			try
			{
				var delay = Math.Round(rand.NextDouble()*20 + 1, 2);
				Console.WriteLine($"{x} started - {delay}");
				await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken.Token);
				var value = rand.Next(100);
				Console.WriteLine($"{x} completed - {value}");
				return value;
			}
			catch (TaskCanceledException)
			{
				Console.WriteLine($"{x} cancelled");
				throw;
			}
		}, cancellationToken.Token));

		var result = await tasks.WhenAny(x => x > 70, cancellationToken.Token);

		cancellationToken.Cancel();

		Console.WriteLine($"Winner: {result.Result}");
	}
}

