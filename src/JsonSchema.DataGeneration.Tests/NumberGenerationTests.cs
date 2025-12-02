using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

public class NumberGenerationTests
{
	[Test]
	public void GenerateNumber()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Number);

		Run(schema, buildOptions);
	}

	[Test]
	public void Minimum()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Number)
			.Minimum(0.2m);

		Run(schema, buildOptions);
	}

	[Test]
	public void ExclusiveMinimum()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Number)
			.ExclusiveMinimum(0.2m);

		Run(schema, buildOptions);
	}

	[Test]
	public void Maximum()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Number)
			.Maximum(0.8m);

		Run(schema, buildOptions);
	}

	[Test]
	public void ExclusiveMaximum()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Number)
			.ExclusiveMaximum(0.8m);

		Run(schema, buildOptions);
	}

	[Test]
	public void MultipleOf()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Number)
			.MultipleOf(0.3m);

		Run(schema, buildOptions);
	}

	[Test]
	public void NotMultipleOf()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Number)
			.Not(new JsonSchemaBuilder().MultipleOf(0.6m));

		Run(schema, buildOptions);
	}

	[Test]
	public void MultipleOfAndNotMultipleOf()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Number)
			.MultipleOf(0.3m)
			.Not(new JsonSchemaBuilder().MultipleOf(0.6m));

		Run(schema, buildOptions);
	}

	[Test]
	[Ignore("flaky")]
	public void MultipleOfWithRange()
	{
		// This test highlights an issue with the number generation:
		// Periodically, the rounding can cause the generation to be
		// out of the range.  Generation is tried 5 times per range.
		// If it fails all 5 times, this test will fail.
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Number)
			.Minimum(10)
			.Maximum(20)
			.MultipleOf(2.3m);

		Run(schema, buildOptions);
	}
}