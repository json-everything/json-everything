using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

public class IntegerGenerationTests
{
	[Test]
	public void GenerateInteger()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer);

		Run(schema);
	}

	[Test]
	public void Minimum()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer)
			.Minimum(10);

		Run(schema);
	}

	[Test]
	public void Maximum()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer)
			.Maximum(20);

		Run(schema);
	}

	[Test]
	public void MultipleOf()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer)
			.MultipleOf(20);

		Run(schema);
	}

	[Test]
	public void DecimalMultipleOf()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer)
			.MultipleOf(0.84m);

		Run(schema);
	}

	[Test]
	public void MultipleOfAndNotMultipleOf()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer)
			.MultipleOf(3)
			.Not(new JsonSchemaBuilder().MultipleOf(6));

		Run(schema);
	}
}