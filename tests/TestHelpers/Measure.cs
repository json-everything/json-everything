using System.Diagnostics;

namespace TestHelpers;

public static class Measure
{
	public static void Run(string name, Action action)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		action();
		var time = stopwatch.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond;
		Console.WriteLine($"{name}: {time}ms");
	}

	public static T Run<T>(string name, Func<T> action)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var value = action();
		var time = stopwatch.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond;
		Console.WriteLine($"{name}: {time}ms");
		return value;
	}

	public static async Task<T> Run<T>(string name, Func<Task<T>> action)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var value = await action();
		var time = stopwatch.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond;
		Console.WriteLine($"{name}: {time}ms");
		return value;
	}
}