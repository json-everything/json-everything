using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

public class UnrecognizedKeywordTests
{
	[Test]
	public void FooIsNotAKeyword()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText, TestEnvironment.SerializerOptions);

		Assert.Multiple(() =>
		{
			Assert.That(schema!.Keywords!, Has.Count.EqualTo(1));
			Assert.That(schema.Keywords!.First(), Is.InstanceOf<UnrecognizedKeyword>());
		});
	}

	[Test]
	public void FooProducesAnAnnotation()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText, TestEnvironment.SerializerOptions);

		var result = schema!.Evaluate(new JsonObject(), new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
		Assert.That(result.Annotations!.Count, Is.EqualTo(1));
		JsonAssert.AreEquivalent("bar", result.Annotations!["foo"]);
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

		result.AssertValid();
		Assert.That(result.Annotations!, Has.Count.EqualTo(2));
		JsonAssert.AreEquivalent(new JsonArray("foo"), result.Annotations!["$unknownKeywords"]);
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

		result.AssertValid();
		Assert.That(result.Annotations!, Has.Count.EqualTo(2));
		JsonAssert.AreEquivalent(new JsonObject { ["foo"] = false }, result.Annotations!["dependencies"]);
		JsonAssert.AreEquivalent(new JsonArray("dependencies"), result.Annotations["$unknownKeywords"]);
	}

	[Test]
	public void FooProducesAnAnnotation_Constructed()
	{
		var schema = new JsonSchemaBuilder()
			.Unrecognized("foo", "bar")
			.Build();

		var result = schema.Evaluate(new JsonObject(), new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
		Assert.That(result.Annotations!, Has.Count.EqualTo(1));
		JsonAssert.AreEquivalent("bar", result.Annotations!["foo"]);
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

		var serializerOptions = new JsonSerializerOptions()
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
		TestConsole.WriteLine(JsonSerializer.Serialize(result, serializerOptions));

		result.AssertValid();
		Assert.That(result.Annotations!, Has.Count.EqualTo(1));
		JsonAssert.AreEquivalent("bar", result.Annotations!["foo"]);
	}

	[Test]
	public void FooIsIncludedInSerialization()
	{
		var schemaText = "{\"foo\":\"bar\"}";

		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText, TestEnvironment.SerializerOptions);

		var reText = JsonSerializer.Serialize(schema, TestEnvironment.SerializerOptions);

		Assert.That(reText, Is.EqualTo(schemaText));	
	}
}