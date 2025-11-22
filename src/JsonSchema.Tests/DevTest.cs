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
		SchemaKeywordRegistry.Default = SchemaKeywordRegistry.Draft201909;

		var schemaText = """
		    {
		      "$id": "https://json-everything.test/generic",
		      "type": "array",
		      "items": { "$recursiveRef": "#"},
		      "$defs": {
		        "fails": {
		          "$recursiveAnchor": true,
		          "type": "boolean"
		        }
		      }
		    }
		    """;

		var schemaJson = JsonDocument.Parse(schemaText).RootElement;
		var schema = Measure.Run("build", () => JsonSchema.Build(schemaJson));

		var typedSchemaText = """
		    {
		      "$id": "https://json-everything.test/specific",
		      "$ref": "generic",
		      "$defs": {
		        "works": {
		          "$recursiveAnchor": true,
		          "type": "string"
		        }
		      }
		    }
		    """;

		var typedSchemaJson = JsonDocument.Parse(typedSchemaText).RootElement;
		var typedSchema = Measure.Run("build", () => JsonSchema.Build(typedSchemaJson));

		var instanceText = """
			[
			  "string 1",
			  "string 2", 
			  42,
			  "string 4"
			]
			""";
		var instance = JsonDocument.Parse(instanceText).RootElement;

		var results = Measure.Run("evaluate", () => typedSchema.Evaluate(instance));

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