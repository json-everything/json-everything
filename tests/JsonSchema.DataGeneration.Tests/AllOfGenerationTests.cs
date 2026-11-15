using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

internal class AllOfGenerationTests
{
	[Test]
	public void AllOfWithMinAndMaxNumber()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AllOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Number),
				new JsonSchemaBuilder().Minimum(10),
				new JsonSchemaBuilder().Maximum(20)
			);

		Run(schema);
	}

	[Test]
	public void AllOfWithDifferentTypesFails()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AllOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Number),
				new JsonSchemaBuilder().Type(SchemaValueType.String)
			);

		RunFailure(schema);
	}

	[Test]
	public void AllOfConflictingMinimaFails()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AllOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Integer).Minimum(100),
				new JsonSchemaBuilder().Maximum(5)
			);

		RunFailure(schema);
	}

	[Test]
	public void AllOfWithTypeRangeAndMultiple()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AllOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Integer),
				new JsonSchemaBuilder().Minimum(10),
				new JsonSchemaBuilder().Maximum(100),
				new JsonSchemaBuilder().MultipleOf(5)
			);

		Run(schema);
	}

	[Test]
	public void AllOfConflictingNestedObjectConstraintsFails()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Required("workflow")
			.AllOf(
				new JsonSchemaBuilder()
					.Properties(("workflow", new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("step", "retries", "title")
						.Properties(
							("step", new JsonSchemaBuilder().Const("start")),
							("retries", new JsonSchemaBuilder().Minimum(10)),
							("title", new JsonSchemaBuilder().Const("ALPHA"))
						)
					)),
				new JsonSchemaBuilder()
					.Properties(("workflow", new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("step", "retries", "title")
						.Properties(
							("step", new JsonSchemaBuilder().Const("finish")),
							("retries", new JsonSchemaBuilder().Maximum(2)),
							("title", new JsonSchemaBuilder().Const("BETA"))
						)
					))
			);

		RunFailure(schema);
	}
}