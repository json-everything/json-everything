using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		var schema1Text = """
		    {
		      "$id": "https://json-everything.test/a",
		      "properties": {
		        "foo": { "type": "string" },
		        "bar": { "$ref": "#anchor" }
		      },
		      "$defs": {
		        "barDef": {
		          "$anchor": "anchor",
		          "type": "number"
		        }
		      }
		    }
		    """;

		var schema1Json = JsonDocument.Parse(schema1Text).RootElement;
		var schema = Measure.Run("build", () => JsonSchema.Build(schema1Json));

		var instanceText = """
			{
			  "foo": 5.4,
			  "bar": null
			}
			""";
		var instance = JsonDocument.Parse(instanceText).RootElement;

		var results = Measure.Run("evaluate", () => schema.Evaluate(instance));

		Console.WriteLine(JsonSerializer.Serialize(results, TestEnvironment.TestOutputSerializerOptions));
	}
}

public static class Measure
{
	public static void Run(string name, Action action)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		action();
		var time = stopwatch.ElapsedMilliseconds;
		Console.WriteLine($"{name}: {time}");
	}

	public static T Run<T>(string name, Func<T> action)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var value = action();
		var time = stopwatch.ElapsedMilliseconds;
		Console.WriteLine($"{name}: {time}");
		return value;
	}

	public static async Task<T> Run<T>(string name, Func<Task<T>> action)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		var value = await action();
		var time = stopwatch.ElapsedMilliseconds;
		Console.WriteLine($"{name}: {time}");
		return value;
	}
}