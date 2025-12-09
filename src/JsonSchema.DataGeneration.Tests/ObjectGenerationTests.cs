using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

public class ObjectGenerationTests
{
	[Test]
	public void GeneratesSingleProperty()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true)
			);

		Run(schema, buildOptions);
	}

	[Test]
	public void GeneratesMultipleProperties()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true),
				("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			);

		Run(schema, buildOptions);
	}

	[Test]
	public void AdditionalPropertiesFalse()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true),
				("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.AdditionalProperties(false);

		Run(schema, buildOptions);
	}

	[Test]
	public void RequiredPropertyNotListedInProperties()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true),
				("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.Required("baz");

		Run(schema, buildOptions);
	}

	[Test]
	public void DefineThreePickTwo()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true),
				("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("baz", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.MaxProperties(2);

		Run(schema, buildOptions);
	}

	[Test]
	public void DefineThreePickTwoButMustContainBaz()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true),
				("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("baz", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Required("baz")
			.MaxProperties(2);

		Run(schema, buildOptions);
	}
}