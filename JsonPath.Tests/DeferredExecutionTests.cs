using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.Path.Tests;

public class DeferredExecutionTests
{
	[Test]
	public void RepeatedQueryOnChangedDataSet()
	{
		var data = new JsonArray { "bob", "sam", "alice" };
		var query = JsonPath.Parse("$[? length(@) > 3 ]");

		var result = query.Evaluate(data);

		Assert.AreEqual(1, result.Matches!.Count);

		data[0] = "sally";

		Assert.AreEqual(2, result.Matches!.Count);
	}

	[Test]
	public void GettingFirstMatchIsQuick()
	{
		var data = Enumerable.Range(0, 1000000).Select(x => (JsonNode?)x).ToJsonArray();
		var query = JsonPath.Parse("$[? @ > 0 ]");

		var result = query.Evaluate(data);
		var first = result.Matches!.First();

		var sw = new Stopwatch();
		sw.Start();
		Assert.AreEqual(1.0, first.Value!.AsValue().GetNumber());
		sw.Stop();

		var shortcutted = sw.ElapsedMilliseconds;

		sw.Reset();
		sw.Start();
		var asList = result.Matches!.ToList();
		first = asList.First();
		Assert.AreEqual(1.0, first.Value!.AsValue().GetNumber());
		sw.Stop();

		Console.WriteLine($"items in full results: {asList.Count}");
		Console.WriteLine($"Time for shortcutted query: {shortcutted}ms");
		Console.WriteLine($"Time for full evaluation:   {sw.ElapsedMilliseconds}ms");
		Assert.Less(shortcutted, sw.ElapsedMilliseconds);
	}
}