using System;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class AutoEvaluationTests
{
	[Test]
	public void SchemaIsKnown()
	{
		var id = "https://json-everything/tests/schema-is-known";

		var schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id(id)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.Build();

		var instance = new JsonObject
		{
			["$schema"] = id,
			["foo"] = 42
		};

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(schema);

		var result = JsonSchema.AutoEvaluate(instance, options);

		result.AssertValid();
	}

	[Test]
	public void SchemaIsNotKnown()
	{
		var id = "https://json-everything/tests/schema-is-not-known";

		var schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id(id)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.Build();

		var instance = new JsonObject
		{
			["$schema"] = id,
			["foo"] = 42
		};

		var options = new EvaluationOptions();

		Assert.Throws<SchemaRefResolutionException>(() => JsonSchema.AutoEvaluate(instance, options));
	}

	[Test]
	public void SchemaKeywordIsMissing()
	{
		var id = "https://json-everything/tests/schema-keyword-is-missing";

		var schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id(id)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.Build();

		var instance = new JsonObject
		{
			["foo"] = 42
		};

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(schema);

		Assert.Throws<ArgumentException>(() => JsonSchema.AutoEvaluate(instance, options));
	}

	[Test]
	public void SchemaIsNotUri()
	{
		var id = "https://json-everything/tests/schema-is-not-uri";

		var schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id(id)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.Build();

		var instance = new JsonObject
		{
			["$schema"] = 42,
			["foo"] = 42
		};

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(schema);

		Assert.Throws<ArgumentException>(() => JsonSchema.AutoEvaluate(instance, options));
	}
}