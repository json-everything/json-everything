using System;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		EvaluationOptions.Default.Log = null!;

		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Title("foo object schema")
			.Properties(
				("foo", new JsonSchemaBuilder()
					.Title("foo's title")
					.Description("foo's description")
					.Type(SchemaValueType.String)
					//.Pattern("^foo ")
					.MinLength(10)
				)
			)
			.Required("foo")
			.AdditionalProperties(false);

		var instance = new JsonObject
		{
			["foo"] = "foo awe;ovinawe",
			["bar"] = "disallowed"
		};

		schema.Compile();

		var sw = new Stopwatch();
		sw.Start();
		var compiledResults = schema.EvaluateCompiled(instance);
		sw.Stop();
		Console.WriteLine(JsonSerializer.Serialize(compiledResults, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
		Console.WriteLine($"elapsed: {sw.ElapsedTicks}");

		sw.Reset();

		sw.Start();
		var legacyResults = schema.Evaluate(instance);
		sw.Stop();
		Console.WriteLine(JsonSerializer.Serialize(legacyResults, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
		Console.WriteLine($"elapsed: {sw.ElapsedTicks}");
	}
}