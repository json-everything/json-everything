using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Json.Schema.Tests.Suite;
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

		Console.WriteLine($"Winner: {result!.Result}");
	}

	class Context
	{
		private readonly Stack<JsonNode?> _locals = new();

		public JsonNode? Root { get; private set; }
		public JsonNode? Local => _locals.Peek();

		public Context(JsonNode root)
		{
			Root = root;
			_locals.Push(root);
		}
		private Context(){}

		public Context Branch(JsonNode? newLocal)
		{
			var branch = new Context
			{
				Root = Root
			};
			branch._locals.Push(Local);
			branch._locals.Push(newLocal);

			return branch;
		}

		public async Task PrintLeafs()
		{
			if (Local is JsonObject obj)
			{
				if (!obj.Any())
					throw new Exception();
				await Task.WhenAll(obj.Select(kvp => Task.Run(async () =>
				{
					var newContext = Branch(kvp.Value);
					await newContext.PrintLeafs();
				})));
			}
			else
				Console.WriteLine(Local);
		}
	}

	[Test]
	[Ignore("")]
	public async Task OtherTest()
	{
		var obj = new JsonObject
		{
			["a"] = new JsonObject
			{
				["a"] = 1.1,
				["b"] = 1.2,
				["c"] = 1.3,
				["d"] = 1.4
			},
			["b"] = new JsonObject
			{
				["a"] = 2.1,
				["b"] = 2.2,
				["c"] = 2.3,
				["d"] = 2.4
			},
			["c"] = new JsonObject
			{
				["a"] = 3.1,
				["b"] = 3.2,
				["c"] = 3.3,
				["d"] = 3.4
			},
			["d"] = new JsonObject
			{
				["a"] = 4.1,
				["b"] = 4.2,
				["c"] = 4.3,
				["d"] = 4.4,
			}
		};

		async Task Selector()
		{
			var context = new Context(obj);
			await context.PrintLeafs();
		}

		var tasks = Enumerable.Range(0, 10000)
			.Select(_ => Task.Run(Selector));

		await Task.WhenAll(tasks);
	}
}

