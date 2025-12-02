using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

public class NotGenerationTests
{
	[Test]
	public void NotAnObject()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Not(new JsonSchemaBuilder().Type(SchemaValueType.Object));

		Run(schema, buildOptions);
	}

	[Test]
	public void DefinitelyAString()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Not(new JsonSchemaBuilder()
				.Type(SchemaValueType.Object |
					  SchemaValueType.Array |
					  SchemaValueType.Integer |
					  SchemaValueType.Number |
					  SchemaValueType.Null |
					  SchemaValueType.Boolean)
			);

		Run(schema, buildOptions);
	}

	[Test]
	public void NotInRange()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer)
			.Not(new JsonSchemaBuilder()
				.Minimum(100)
				.Maximum(500)
			);

		Run(schema, buildOptions);
	}

	[Test]
	public void NumberNotInSubrange()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer)
			.Minimum(0)
			.Maximum(1000)
			.Not(new JsonSchemaBuilder()
				.Minimum(100)
				.Maximum(500)
			);

		Run(schema, buildOptions);
	}

	// TODO: verify that array generation is checking bound type for min/max items (and props, too)
	[Test]
	public void ItemCountNotInRange()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.MaxItems(20)
			.Not(new JsonSchemaBuilder()
				.MinItems(5)
				.MaxItems(10)
			);

		Run(schema, buildOptions);
	}

	[Test]
	public void ItemsAreNotString()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.MinItems(1)
			.Not(new JsonSchemaBuilder()
				.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			);

		Run(schema, buildOptions);
	}

	[Test]
	[Ignore("flaky, not sure why")]
	public void DoesNotContainString()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Not(new JsonSchemaBuilder()
				.Contains(new JsonSchemaBuilder().Type(SchemaValueType.String))
			);

		Run(schema, buildOptions);
	}

	[Test]
	public void ContainsAtLeastOneNonstring()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Contains(new JsonSchemaBuilder()
				.Not(new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
				)
			);

		Run(schema, buildOptions);
	}

	[Test]
	[Ignore("flaky, not sure why")]
	public void DoesNotContainStringOrNull()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.AllOf(
				new JsonSchemaBuilder()
					.Not(new JsonSchemaBuilder()
						.Contains(new JsonSchemaBuilder().Type(SchemaValueType.String)
						)
					),
				new JsonSchemaBuilder()
					.Not(new JsonSchemaBuilder()
						.Contains(new JsonSchemaBuilder().Type(SchemaValueType.Null)
						)
					)
			);

		Run(schema, buildOptions);
	}

	[Test]
	public void AnObjectThatDoesNotContainAFooPropertyWithoutSpecifyingType()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Not(new JsonSchemaBuilder().Required("foo"));

		Run(schema, buildOptions);
	}
}