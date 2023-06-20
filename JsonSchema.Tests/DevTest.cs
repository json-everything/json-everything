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
		var tasks = Enumerable.Range(0, 10).Select(async x =>
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
			catch (TaskCanceledException e)
			{
				Console.WriteLine($"{x} cancelled");
				throw;
			}
		});

		var result = await tasks.WhenAny(x => x > 70);

		cancellationToken.Cancel();

		Console.WriteLine($"Winner: {result.Result}");
	}
}

static class TaskEx
{
	public static async Task<Task<T>> WhenAny<T>(this IEnumerable<Task<T>> tasks, Func<T, bool> predicate)
	{
		var list = tasks.ToList();
		T result;
		Task<T> task;
		do
		{
			task = await Task.WhenAny(list);
			result = task.Result;
			list.Remove(task);
		} while (list.Any() && !predicate(result));

		return task;
	}
}