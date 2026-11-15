using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

public class ArrayGenerationTests
{
	[Test]
	public void GenerateArrayOfNumbers()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(20)
				.MultipleOf(2)
			)
			.MinItems(3)
			.MaxItems(10);

		Run(schema);
	}

	[Test]
	public void GenerateArrayOfNumbersNoMax()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(20)
				.MultipleOf(2)
			)
			.MinItems(3);

		Run(schema);
	}

	[Test]
	public void GenerateArrayOfNumbersWithAllOf()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.AllOf(
				new JsonSchemaBuilder()
					.Items(new JsonSchemaBuilder().MultipleOf(3)),
				new JsonSchemaBuilder()
					.Items(new JsonSchemaBuilder().MultipleOf(2))
			)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(50)
			)
			.MinItems(3)
			.MaxItems(10);

		Run(schema);
	}

	[Test]
	public void GenerateArrayOfNumbersWithAnyOf()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.AnyOf(
				new JsonSchemaBuilder()
					.Items(new JsonSchemaBuilder().MultipleOf(3)),
				new JsonSchemaBuilder()
					.Items(new JsonSchemaBuilder().MultipleOf(2))
			)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(50)
			)
			.MinItems(3)
			.MaxItems(10);

		Run(schema);
	}

	[Test]
	public void GenerateArrayThatContains100()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(100)
			)
			.Contains(new JsonSchemaBuilder()
				.Minimum(100)
			)
			.MinItems(3)
			.MaxItems(10);

		Run(schema);
	}

	[Test]
	public void GenerateArrayThatContainsAtLeastTwo100s()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(100)
			)
			.Contains(new JsonSchemaBuilder()
				.Minimum(100)
			)
			.MinContains(2)
			.MinItems(3)
			.MaxItems(10);

		Run(schema);
	}

	[Test]
	public void GenerateArrayThatContainsAtMostThree100s()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(100)
			)
			.Contains(new JsonSchemaBuilder()
				.Minimum(100)
			)
			.MaxContains(3)
			.MinItems(3)
			.MaxItems(10);

		Run(schema);
	}

	[Test]
	public void GenerateArrayThatContainsBetweenTwoAndFive100s()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(100)
			)
			.Contains(new JsonSchemaBuilder()
				.Minimum(100)
			)
			.MinContains(2)
			.MaxContains(5)
			.MinItems(3)
			.MaxItems(20);

		Run(schema);
	}

	[Test]
	public void GenerateArrayWhereMinContainsIsMoreThanMaxItems_Fails()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(100)
			)
			.Contains(new JsonSchemaBuilder()
				.Minimum(100)
			)
			.MinContains(20)
			.MinItems(3)
			.MaxItems(10);

		RunFailure(schema);
	}

	[Test]
	public void GenerateSequentialArray()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.PrefixItems(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Minimum(10)
					.Maximum(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.MinLength(10)
					.MaxLength(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Boolean)
			)
			.MinItems(3)
			.MaxItems(3);

		Run(schema);
	}

	[Test]
	public void GenerateSequentialArrayPlusMore()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.PrefixItems(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Minimum(10)
					.Maximum(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.MinLength(10)
					.MaxLength(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Boolean)
			)
			.MinItems(5)
			.MaxItems(10);

		Run(schema);
	}

	[Test]
	public void GenerateSequentialArrayPlusSpecifiedMore()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.PrefixItems(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Minimum(10)
					.Maximum(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.MinLength(10)
					.MaxLength(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Boolean)
			)
			.MinItems(5)
			.MaxItems(10)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(100)
				.Maximum(200)
			);

		Run(schema);
	}

	[Test]
	public void GenerateSequentialArrayWithFewerItems()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.PrefixItems(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Minimum(10)
					.Maximum(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.MinLength(10)
					.MaxLength(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Boolean)
			)
			.MaxItems(2);

		Run(schema);
	}

	[Test]
	public void GenerateSequentialArrayPlusMoreAndContains()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.PrefixItems(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Minimum(10)
					.Maximum(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.MinLength(10)
					.MaxLength(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Boolean)
			)
			.MinItems(5)
			.MaxItems(10)
			.Contains(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(50)
				.Maximum(100)
			);

		Run(schema);
	}

	[Test]
	public void GenerateSequentialArrayWithNoMoreItems()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.PrefixItems(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Minimum(10)
					.Maximum(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.MinLength(10)
					.MaxLength(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Boolean)
			)
			.Items(false);

		Run(schema);
	}

	[Test]
	public void GenerateSequentialArrayWithNoMoreItemsAndMinItems()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.PrefixItems(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Minimum(10)
					.Maximum(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.MinLength(10)
					.MaxLength(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Boolean)
			)
			.MinItems(3)
			.Items(false);

		Run(schema);
	}

	[Test]
	public void GenerateSequentialArrayWithNoMoreItemsAndMinItemsGreaterThanSequential()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.PrefixItems(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Minimum(10)
					.Maximum(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.MinLength(10)
					.MaxLength(20),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Boolean)
			)
			.MinItems(4)
			.Items(false);

		RunFailure(schema);
	}

	[Test]
	public void AllOfWithCompatibleSequentialItemsGenerates()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AllOf(
				new JsonSchemaBuilder().PrefixItems(
					new JsonSchemaBuilder().Type(SchemaValueType.Integer).Minimum(10),
					new JsonSchemaBuilder().Type(SchemaValueType.String).MinLength(3)
				),
				new JsonSchemaBuilder().PrefixItems(
					new JsonSchemaBuilder().Type(SchemaValueType.Integer).Maximum(20).MultipleOf(2),
					new JsonSchemaBuilder().Type(SchemaValueType.String).MaxLength(10)
				)
			)
			.Type(SchemaValueType.Array)
			.MinItems(2)
			.MaxItems(2);

		Run(schema);
	}

	[Test]
	public void AllOfWithConflictingSequentialItemsFails()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AllOf(
				new JsonSchemaBuilder().PrefixItems(
					new JsonSchemaBuilder().Type(SchemaValueType.Integer)
				),
				new JsonSchemaBuilder().PrefixItems(
					new JsonSchemaBuilder().Type(SchemaValueType.String)
				)
			)
			.Type(SchemaValueType.Array)
			.MinItems(1)
			.MaxItems(1);

		RunFailure(schema);
	}
}