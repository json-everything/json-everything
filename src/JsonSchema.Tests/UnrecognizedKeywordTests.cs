using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Json.More;
using Json.Schema.Keywords;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

public class UnrecognizedKeywordTests
{
	[Test]
	public void FooIsNotAKeyword()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var buildOptions = new BuildOptions { Dialect = Dialect.Draft202012 };
		var schema = JsonSchema.FromText(schemaText, buildOptions);

		Assert.Multiple(() =>
		{
			Assert.That(schema.Root.Keywords, Has.Length.EqualTo(1));
			Assert.That(schema.Root.Keywords.First().Handler, Is.InstanceOf<AnnotationKeyword>());
		});
	}

	[Test]
	public void FooProducesAnAnnotation()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var buildOptions = new BuildOptions { Dialect = Dialect.Draft202012 };
		var schema = JsonSchema.FromText(schemaText, buildOptions);

		var instance = JsonDocument.Parse("{}").RootElement;
		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
		Assert.That(result.Annotations!.Count, Is.EqualTo(1));
		JsonAssert.AreEquivalent("bar".AsJsonElement(), result.Annotations!["foo"]);
	}

	[Test]
	public void UnknownKeywordAnnotationIsProduced()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var buildOptions = new BuildOptions { Dialect = Dialect.Draft202012 };
		var schema = JsonSchema.FromText(schemaText, buildOptions);

		var instance = JsonDocument.Parse("{}").RootElement;
		var result = schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			AddAnnotationForUnknownKeywords = true
		});

		result.AssertValid();
		Assert.That(result.Annotations!, Has.Count.EqualTo(2));
		var expectedArray = JsonDocument.Parse("[\"foo\"]").RootElement;
		JsonAssert.AreEquivalent(expectedArray, result.Annotations!["$unknownKeywords"]);
	}

	[Test]
	public void AnnotationsProducedForKnownButUnused()
	{
		var schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Dependencies(new Dictionary<string, SchemaOrPropertyList>
			{
				["foo"] = (JsonSchemaBuilder)false
			});

		var instance = JsonDocument.Parse("{\"bar\": 5}").RootElement;
		var result = schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			AddAnnotationForUnknownKeywords = true
		});

		result.AssertValid();
		Assert.That(result.Annotations!, Has.Count.EqualTo(2));
		var expectedDeps = JsonDocument.Parse("{\"foo\": false}").RootElement;
		JsonAssert.AreEquivalent(expectedDeps, result.Annotations!["dependencies"]);
		var expectedKeywords = JsonDocument.Parse("[\"dependencies\"]").RootElement;
		JsonAssert.AreEquivalent(expectedKeywords, result.Annotations["$unknownKeywords"]);
	}

	[Test]
	public void FooProducesAnAnnotation_Constructed()
	{
		var buildOptions = new BuildOptions { Dialect = Dialect.Draft202012 };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Unrecognized("foo", "bar")
			.Build();

		var instance = JsonDocument.Parse("{}").RootElement;
		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
		Assert.That(result.Annotations!, Has.Count.EqualTo(1));
		JsonAssert.AreEquivalent("bar".AsJsonElement(), result.Annotations!["foo"]);
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

		var instance = JsonDocument.Parse("{}").RootElement;

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
		JsonAssert.AreEquivalent("bar".AsJsonElement(), result.Annotations!["foo"]);
	}

	[Test]
	public void FooIsIncludedInSerialization()
	{
		var schemaText = "{\"foo\":\"bar\"}";

		var buildOptions = new BuildOptions { Dialect = Dialect.Draft202012 };
		var schema = JsonSchema.FromText(schemaText, buildOptions);

		var reText = JsonSerializer.Serialize(schema, TestEnvironment.SerializerOptions);

		Assert.That(reText, Is.EqualTo(schemaText));	
	}

	[Test]
	public void FooIsNotAKeyword_DefaultDialect_ThrowsException()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var ex = Assert.Throws<JsonSchemaException>(() => JsonSchema.FromText(schemaText));
		Assert.That(ex!.Message, Does.Contain("Unknown keywords (foo) are disallowed for this dialect."));
	}

	[Test]
	public void FooProducesAnAnnotation_DefaultDialect_ThrowsException()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var ex = Assert.Throws<JsonSchemaException>(() => JsonSchema.FromText(schemaText));
		Assert.That(ex!.Message, Does.Contain("Unknown keywords (foo) are disallowed for this dialect."));
	}

	[Test]
	public void UnknownKeywordAnnotationIsProduced_DefaultDialect_ThrowsException()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var ex = Assert.Throws<JsonSchemaException>(() => JsonSchema.FromText(schemaText));
		Assert.That(ex!.Message, Does.Contain("Unknown keywords (foo) are disallowed for this dialect."));
	}

	[Test]
	public void FooProducesAnAnnotation_Constructed_DefaultDialect_ThrowsException()
	{
		var ex = Assert.Throws<JsonSchemaException>(() => new JsonSchemaBuilder()
			.Unrecognized("foo", "bar")
			.Build());
		Assert.That(ex!.Message, Does.Contain("Unknown keywords (foo) are disallowed for this dialect."));
	}

	[Test]
	public void FooIsIncludedInSerialization_DefaultDialect_ThrowsException()
	{
		var schemaText = "{\"foo\":\"bar\"}";

		var ex = Assert.Throws<JsonSchemaException>(() => JsonSchema.FromText(schemaText));
		Assert.That(ex!.Message, Does.Contain("Unknown keywords (foo) are disallowed for this dialect."));
	}
}