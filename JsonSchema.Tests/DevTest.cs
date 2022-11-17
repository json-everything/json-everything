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
			.Id("https://somethingrandom.here/schema")
			.OneOf(
				new JsonSchemaBuilder().Id("https://something.else/schema").Type(SchemaValueType.Integer).MultipleOf(3),
				new JsonSchemaBuilder().Ref("http://inside.def/schema")
			)
			.Defs(
				("target", new JsonSchemaBuilder().Id("http://inside.def/schema").Type(SchemaValueType.String))
			);

		JsonNode instance = "hello";

		schema.Compile();

		var sw = new Stopwatch();
		sw.Start();
		var compiledResults = schema.EvaluateCompiled(instance);
		sw.Stop();
		Console.WriteLine(JsonSerializer.Serialize(compiledResults, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
		Console.WriteLine($"elapsed: {sw.ElapsedTicks}");

		sw.Reset();

		sw.Start();
		var legacyResults = schema.Evaluate(instance, new EvaluationOptions{OutputFormat = OutputFormat.Basic});
		sw.Stop();
		Console.WriteLine(JsonSerializer.Serialize(legacyResults, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
		Console.WriteLine($"elapsed: {sw.ElapsedTicks}");
	}

	[Test]
	[Ignore("This is just me playing with the Uri type.  I want it documented for future-me.")]
	public void UriTesting()
	{
		var baseUri = new Uri("http://my.uri/schema/folder");

		Console.WriteLine(new Uri(baseUri, "sibling/path"));
		Console.WriteLine(new Uri(baseUri, "#/json/pointer"));
		Console.WriteLine(new Uri(baseUri, "#anchor"));
		Console.WriteLine(new Uri(baseUri, "sibling/with#/json/pointer"));
		Console.WriteLine(new Uri(baseUri, "sibling/with#anchor"));
		Console.WriteLine(new Uri(baseUri, "/rooted/path"));
		Console.WriteLine(new Uri(baseUri, "/rooted/path/with#/json/pointer"));
		Console.WriteLine(new Uri(baseUri, "/rooted/path/with#anchor"));
		Console.WriteLine(new Uri(baseUri, "http://somewhere.else/entirely"));
		//http://my.uri/schema/sibling/path
		//http://my.uri/schema/folder#/json/pointer
		//http://my.uri/schema/folder#anchor
		//http://my.uri/schema/sibling/with#/json/pointer
		//http://my.uri/schema/sibling/with#anchor
		//http://my.uri/rooted/path
		//http://my.uri/rooted/path/with#/json/pointer
		//http://my.uri/rooted/path/with#anchor
		//http://somewhere.else/entirely

		Console.WriteLine();
		baseUri = new Uri("http://my.uri/schema/folderwithslash/");

		Console.WriteLine(new Uri(baseUri, "sibling/path"));
		Console.WriteLine(new Uri(baseUri, "#/json/pointer"));
		Console.WriteLine(new Uri(baseUri, "#anchor"));
		Console.WriteLine(new Uri(baseUri, "sibling/with#/json/pointer"));
		Console.WriteLine(new Uri(baseUri, "sibling/with#anchor"));
		Console.WriteLine(new Uri(baseUri, "/rooted/path"));
		Console.WriteLine(new Uri(baseUri, "/rooted/path/with#/json/pointer"));
		Console.WriteLine(new Uri(baseUri, "/rooted/path/with#anchor"));
		Console.WriteLine(new Uri(baseUri, "http://somewhere.else/entirely"));
		//http://my.uri/schema/folderwithslash/sibling/path
		//http://my.uri/schema/folderwithslash/#/json/pointer
		//http://my.uri/schema/folderwithslash/#anchor
		//http://my.uri/schema/folderwithslash/sibling/with#/json/pointer
		//http://my.uri/schema/folderwithslash/sibling/with#anchor
		//http://my.uri/rooted/path
		//http://my.uri/rooted/path/with#/json/pointer
		//http://my.uri/rooted/path/with#anchor
		//http://somewhere.else/entirely

	}
}