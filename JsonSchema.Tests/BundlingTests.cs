using System;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class BundlingTests
{
	[Test]
	public void Draft201909ContainsDraft202012_InnerShouldProcess202012Keywords()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft201909Id)
			.Id("https://json-everything/draft2019schema")
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Ref("draft2020schema"))
			.Defs(
				("draft2020schema", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft202012Id)
					.Id("https://json-everything/draft2020schema")
					.Type(SchemaValueType.Array)
					// this should be processed
					.PrefixItems(new JsonSchemaBuilder().Type(SchemaValueType.Number))
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
			);

		var instance = JsonDocument.Parse("[[1, \"other string\"]]");

		var result = schema.Evaluate(instance.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
	}

	[Test]
	public void Draft202012ContainsDraft201909_InnerShouldIgnore202012Keywords()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/draft2020schema")
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Ref("draft2019schema"))
			.Defs(
				("draft2019schema", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft201909Id)
					.Id("https://json-everything/draft2019schema")
					.Type(SchemaValueType.Array)
					// this should be not processed
					.PrefixItems(new JsonSchemaBuilder().Type(SchemaValueType.Number))
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
			);

		var instance = JsonDocument.Parse("[[\"one string\", \"other string\"]]");

		var result = schema.Evaluate(instance.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
	}

	[Test]
	public void BundleMultipleDocuments()
	{
		JsonSchema foo = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/foo")
			.Type(SchemaValueType.Object)
			.Properties(
				("bar", new JsonSchemaBuilder().Ref("bar"))
			);

		JsonSchema bar = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/bar")
			.Type(SchemaValueType.String);

		// for reference
		// ReSharper disable once UnusedVariable
		JsonSchema expected = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/foo(bundled)")
			.Defs(
				("foo", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft202012Id)
					.Id("https://json-everything/foo")
					.Type(SchemaValueType.Object)
					.Properties(
						("bar", new JsonSchemaBuilder().Ref("bar"))
					)
				),
				("bar", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft202012Id)
					.Id("https://json-everything/bar")
					.Type(SchemaValueType.String)
				)
			)
			.Ref("foo");

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(bar);
		var actual = foo.Bundle(options);

		Console.WriteLine(JsonSerializer.Serialize(foo, TestEnvironment.SerializerOptions));
		Console.WriteLine(JsonSerializer.Serialize(bar, TestEnvironment.SerializerOptions));
		Console.WriteLine(JsonSerializer.Serialize(actual, TestEnvironment.SerializerOptions));

		Assert.That(() => actual.GetDefs()!.Values.Any(x => x.GetId()!.OriginalString == "https://json-everything/foo"));
		Assert.That(() => actual.GetDefs()!.Values.Any(x => x.GetId()!.OriginalString == "https://json-everything/bar"));
	}

	[Test]
	public void BundleMultipleDocumentsWithAlreadyBundledSubschema()
	{
		JsonSchema foo = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/foo")
			.Defs(
				("baz", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft202012Id)
					.Id("https://json-everything/baz")
					.Type(SchemaValueType.Integer)
				)
			)
			.Type(SchemaValueType.Object)
			.Properties(
				("bar", new JsonSchemaBuilder().Ref("bar")),
				("baz", new JsonSchemaBuilder().Ref("baz"))
			);

		JsonSchema bar = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/bar")
			.Type(SchemaValueType.String);

		// for reference
		// ReSharper disable once UnusedVariable
		JsonSchema expected = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/foo(bundled)")
			.Defs(
				("foo", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft202012Id)
					.Id("https://json-everything/foo")
					.Defs(
						("baz", new JsonSchemaBuilder()
							.Schema(MetaSchemas.Draft202012Id)
							.Id("https://json-everything/baz")
							.Type(SchemaValueType.Integer)
						)
					)
					.Type(SchemaValueType.Object)
					.Properties(
						("bar", new JsonSchemaBuilder().Ref("bar")),
						("baz", new JsonSchemaBuilder().Ref("baz"))
					)
				),
				("bar", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft202012Id)
					.Id("https://json-everything/bar")
					.Type(SchemaValueType.String)
				)
			)
			.Ref("foo");

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(bar);
		var actual = foo.Bundle(options);

		Console.WriteLine(JsonSerializer.Serialize(foo, TestEnvironment.SerializerOptions));
		Console.WriteLine(JsonSerializer.Serialize(bar, TestEnvironment.SerializerOptions));
		Console.WriteLine(JsonSerializer.Serialize(actual, TestEnvironment.SerializerOptions));

		Assert.That(() => actual.GetDefs()!.Values.Any(x => x.GetId()!.OriginalString == "https://json-everything/foo"));
		Assert.That(() => actual.GetDefs()!.Values.Any(x => x.GetId()!.OriginalString == "https://json-everything/bar"));
		Assert.That(() => actual.GetDefs()!.Values.All(x => x.GetId()!.OriginalString != "https://json-everything/baz"));
	}

	[Test]
	public void BundleMultipleDocumentsMultipleRefs()
	{
		JsonSchema foo = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/foo")
			.Type(SchemaValueType.Object)
			.Properties(
				("bar", new JsonSchemaBuilder().Ref("bar"))
			);

		JsonSchema bar = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/bar")
			.Type(SchemaValueType.Object)
			.Properties(
				("baz", new JsonSchemaBuilder().Ref("baz"))
			);

		JsonSchema baz = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/baz")
			.Type(SchemaValueType.String);

		// for reference
		// ReSharper disable once UnusedVariable
		JsonSchema expected = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/foo(bundled)")
			.Defs(
				("foo", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft202012Id)
					.Id("https://json-everything/foo")
					.Type(SchemaValueType.Object)
					.Properties(
						("bar", new JsonSchemaBuilder().Ref("bar"))
					)
				),
				("bar", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft202012Id)
					.Id("https://json-everything/bar")
					.Type(SchemaValueType.Object)
					.Properties(
						("baz", new JsonSchemaBuilder().Ref("baz"))
					)
				),
				("baz", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft202012Id)
					.Id("https://json-everything/baz")
					.Type(SchemaValueType.String)
				)
			)
			.Ref("foo");

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(bar);
		options.SchemaRegistry.Register(baz);
		var actual = foo.Bundle(options);

		Console.WriteLine(JsonSerializer.Serialize(foo, TestEnvironment.SerializerOptions));
		Console.WriteLine(JsonSerializer.Serialize(bar, TestEnvironment.SerializerOptions));
		Console.WriteLine(JsonSerializer.Serialize(baz, TestEnvironment.SerializerOptions));
		Console.WriteLine(JsonSerializer.Serialize(actual, TestEnvironment.SerializerOptions));

		Assert.That(() => actual.GetDefs()!.Values.Any(x => x.GetId()!.OriginalString == "https://json-everything/foo"));
		Assert.That(() => actual.GetDefs()!.Values.Any(x => x.GetId()!.OriginalString == "https://json-everything/bar"));
		Assert.That(() => actual.GetDefs()!.Values.Any(x => x.GetId()!.OriginalString == "https://json-everything/baz"));
	}
}