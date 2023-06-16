using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

public class ArrayGenerationTests
{
	[Test]
	public async Task GenerateArrayOfNumbers()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(20)
				.MultipleOf(2)
			)
			.MinItems(3)
			.MaxItems(10);

		await Run(schema);
	}

	[Test]
	public async Task GenerateArrayOfNumbersNoMax()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10)
				.Maximum(20)
				.MultipleOf(2)
			)
			.MinItems(3);

		await Run(schema);
	}

	[Test]
	public async Task GenerateArrayOfNumbersWithAllOf()
	{
		JsonSchema schema = new JsonSchemaBuilder()
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

		await Run(schema);
	}

	[Test]
	public async Task GenerateArrayOfNumbersWithAnyOf()
	{
		JsonSchema schema = new JsonSchemaBuilder()
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

		await Run(schema);
	}

	[Test]
	public async Task GenerateArrayThatContains100()
	{
		JsonSchema schema = new JsonSchemaBuilder()
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

		await Run(schema);
	}

	[Test]
	public async Task GenerateArrayThatContainsAtLeastTwo100s()
	{
		JsonSchema schema = new JsonSchemaBuilder()
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

		await Run(schema);
	}

	[Test]
	public async Task GenerateArrayThatContainsAtMostThree100s()
	{
		JsonSchema schema = new JsonSchemaBuilder()
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

		await Run(schema);
	}

	[Test]
	public async Task GenerateArrayThatContainsBetweenTwoAndFive100s()
	{
		JsonSchema schema = new JsonSchemaBuilder()
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

		await Run(schema);
	}

	[Test]
	public void GenerateArrayWhereMinContainsIsMoreThanMaxItems_Fails()
	{
		JsonSchema schema = new JsonSchemaBuilder()
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
	public async Task GenerateSequentialArray()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.PrefixItems(new JsonSchema[]
			{
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
			})
			.MinItems(3)
			.MaxItems(3);

		await Run(schema);
	}

	[Test]
	public async Task GenerateSequentialArrayPlusMore()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.PrefixItems(new JsonSchema[]
			{
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
			})
			.MinItems(5)
			.MaxItems(10);

		await Run(schema);
	}

	[Test]
	public async Task GenerateSequentialArrayPlusSpecifiedMore()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.PrefixItems(new JsonSchema[]
			{
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
			})
			.MinItems(5)
			.MaxItems(10)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(100)
				.Maximum(200)
			);

		await Run(schema);
	}

	[Test]
	public async Task GenerateSequentialArrayWithFewerItems()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.PrefixItems(new JsonSchema[]
			{
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
			})
			.MaxItems(2);

		await Run(schema);
	}

	[Test]
	public async Task GenerateSequentialArrayPlusMoreAndContains()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.PrefixItems(new JsonSchema[]
			{
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
			})
			.MinItems(5)
			.MaxItems(10)
			.Contains(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(50)
				.Maximum(100)
			);

		await Run(schema);
	}

	[Test]
	public async Task GenerateSequentialArrayWithNoMoreItems()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.PrefixItems(new JsonSchema[]
			{
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
			})
			.Items(false);

		await Run(schema);
	}

	[Test]
	public async Task GenerateSequentialArrayWithNoMoreItemsAndMinItems()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.PrefixItems(new JsonSchema[]
			{
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
			})
			.MinItems(3)
			.Items(false);

		await Run(schema);
	}

	[Test]
	public void GenerateSequentialArrayWithNoMoreItemsAndMinItemsGreaterThanSequential()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.PrefixItems(new JsonSchema[]
			{
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
			})
			.MinItems(4)
			.Items(false);

		RunFailure(schema);
	}
}