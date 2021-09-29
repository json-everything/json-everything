using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Json.Schema.Benchmark
{
	class Program
	{
		static async Task Main(string[] args)
		{
			await RunFull(1);
			Console.WriteLine();
			await RunFull(10000);
		}

		private static async Task RunFull(int iterations)
		{
			var schemaText = await File.ReadAllTextAsync("SimplePropsSchema.json");
			var instanceText = await File.ReadAllTextAsync("SimplePropsInstance.json");

			await Time($"NJsonSchema {iterations} runs", async () =>
			{
				var schema = await NJsonSchema.JsonSchema.FromJsonAsync(schemaText);
				return !schema.Validate(instanceText).Any();
			}, iterations);

			await Time($"json-everything {iterations} runs", async () =>
			{
				var schema = JsonSchema.FromText(schemaText);
				var instance = JsonDocument.Parse(instanceText);

				return schema.Validate(instance.RootElement).IsValid;
			}, iterations);
		}

		private static async Task Time(string testName, Func<Task<bool>> action, int iterations)
		{
			var watch = new Stopwatch();
			watch.Start();

			for (int i = 0; i < iterations; i++)
			{
				await action();
			}

			watch.Stop();

			Console.Write($"{testName}: ");
			Console.WriteLine(watch.ElapsedMilliseconds);
		}
	}
}
