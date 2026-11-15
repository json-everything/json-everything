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

		Run(schema);
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

		Run(schema);
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

		Run(schema);
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

		Run(schema);
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

		Run(schema);
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

		Run(schema);
	}

	[Test]
	public void MinProperties()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.MinProperties(3);

		Run(schema);
	}

	[Test]
	public void MinAndMaxProperties()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.MinProperties(2)
			.MaxProperties(4);

		Run(schema);
	}

	[Test]
	public void AdditionalPropertiesWithSchema()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("id", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.String));

		Run(schema);
	}

	[Test]
	public void PropertyWithPatternConstraint()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("name", new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Pattern(@"^[A-Z][a-z]+$")
					.MinLength(2)
					.MaxLength(20))
			)
			.Required("name");

		Run(schema);
	}

	[Test]
	public void MinPropertiesExceedsMaxPropertiesFails()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.MinProperties(3)
			.MaxProperties(1);

		RunFailure(schema);
	}

	[Test]
	public void PropertyNamesPatternForGeneratedProperties()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.MinProperties(3)
			.MaxProperties(5)
			.PropertyNames(new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Pattern("^[a-z]{3,8}$")
			);

		Run(schema);
	}

	[Test]
	public void PropertyNamesLengthForGeneratedProperties()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.MinProperties(2)
			.MaxProperties(4)
			.PropertyNames(new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.MinLength(4)
				.MaxLength(6)
			);

		Run(schema);
	}

	[Test]
	public void RequiredNameViolatesPropertyNamesFails()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("A", true)
			)
			.Required("A")
			.PropertyNames(new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Pattern("^[a-z]+$")
			);

		RunFailure(schema);
	}

	[Test]
	public void PropertyNamesFromEnum()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.MinProperties(2)
			.MaxProperties(2)
			.PropertyNames(new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Enum("alpha", "beta")
			)
			.AdditionalProperties(true);

		Run(schema);
	}
}