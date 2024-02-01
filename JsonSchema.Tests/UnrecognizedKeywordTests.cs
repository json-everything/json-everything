using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class UnrecognizedKeywordTests
{
	[Test]
	public void FooIsNotAKeyword()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText, TestEnvironment.SerializerOptions);

		Assert.AreEqual(1, schema!.Keywords!.Count);
		Assert.IsInstanceOf<UnrecognizedKeyword>(schema.Keywords.First());
	}

	[Test]
	public void FooProducesAnAnnotation()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText, TestEnvironment.SerializerOptions);

		var result = schema!.Evaluate(new JsonObject(), new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		Assert.IsTrue(result.IsValid);
		Assert.AreEqual(1, result.Annotations!.Count);
		Assert.IsTrue(((JsonNode?)"bar").IsEquivalentTo(result.Annotations["foo"]));
	}

	[Test]
	public void UnknownKeywordAnnotationIsProduced()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText, TestEnvironment.SerializerOptions);

		var result = schema!.Evaluate(new JsonObject(), new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			AddAnnotationForUnknownKeywords = true
		});

		Assert.IsTrue(result.IsValid);
		Assert.AreEqual(2, result.Annotations!.Count);
		Assert.IsTrue(new JsonArray("foo").IsEquivalentTo(result.Annotations["$unknownKeywords"]));
	}

	[Test]
	public void AnnotationsProducedForKnownButUnused()
	{
		var schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Dependencies(new Dictionary<string, SchemaOrPropertyList>
			{
				["foo"] = (JsonSchema)false
			});

		var instance = new JsonObject { ["bar"] = 5 };
		var result = schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			AddAnnotationForUnknownKeywords = true
		});

		Assert.IsTrue(result.IsValid);
		Assert.AreEqual(2, result.Annotations!.Count);
		Assert.IsTrue(new JsonObject { ["foo"] = false }.IsEquivalentTo(result.Annotations["dependencies"]));
		Assert.IsTrue(new JsonArray("dependencies").IsEquivalentTo(result.Annotations["$unknownKeywords"]));
	}

	[Test]
	public void FooProducesAnAnnotation_Constructed()
	{
		var schema = new JsonSchemaBuilder()
			.Unrecognized("foo", "bar")
			.Build();

		var result = schema.Evaluate(new JsonObject(), new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		Assert.IsTrue(result.IsValid);
		Assert.AreEqual(1, result.Annotations!.Count);
		Assert.IsTrue(((JsonNode?)"bar").IsEquivalentTo(result.Annotations["foo"]));
	}

	[Test]
	public void FooProducesAnAnnotation_WithSchemaKeyword()
	{
		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};
		var schema = new JsonSchemaBuilder()
			.Id("https://example.com")
			.Schema(MetaSchemas.Draft202012Id)
			.Unrecognized("foo", "bar")
			.Build();

		var instance = new JsonObject();

		var result = schema.Evaluate(instance, options);

		var serializerOptions = new JsonSerializerOptions(TestEnvironment.SerializerOptions)
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		}.WithJsonSchema();
		Console.WriteLine(JsonSerializer.Serialize(result, serializerOptions));

		Assert.IsTrue(result.IsValid);
		Assert.AreEqual(1, result.Annotations!.Count);
		Assert.IsTrue(((JsonNode?)"bar").IsEquivalentTo(result.Annotations["foo"]));
	}

	[Test]
	public void FooIsIncludedInSerialization()
	{
		var schemaText = "{\"foo\":\"bar\"}";

		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText, TestEnvironment.SerializerOptions);

		var reText = JsonSerializer.Serialize(schema, TestEnvironment.SerializerOptions);

		Assert.AreEqual(schemaText, reText);	
	}
}