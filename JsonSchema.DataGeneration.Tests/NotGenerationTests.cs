using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

public class NotGenerationTests
{
	[Test]
	public async Task NotAnObject()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Not(new JsonSchemaBuilder().Type(SchemaValueType.Object));

		await Run(schema);
	}

	[Test]
	public async Task DefinitelyAString()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Not(new JsonSchemaBuilder()
				.Type(SchemaValueType.Object |
					  SchemaValueType.Array |
					  SchemaValueType.Integer |
					  SchemaValueType.Number |
					  SchemaValueType.Null |
					  SchemaValueType.Boolean)
			);

		await Run(schema);
	}

	[Test]
	public async Task NotInRange()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer)
			.Not(new JsonSchemaBuilder()
				.Minimum(100)
				.Maximum(500)
			);

		await Run(schema);
	}

	[Test]
	public async Task NumberNotInSubrange()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer)
			.Minimum(0)
			.Maximum(1000)
			.Not(new JsonSchemaBuilder()
				.Minimum(100)
				.Maximum(500)
			);

		await Run(schema);
	}

	// TODO: verify that array generation is checking bound type for min/max items (and props, too)
	[Test]
	public async Task ItemCountNotInRange()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.MaxItems(20)
			.Not(new JsonSchemaBuilder()
				.MinItems(5)
				.MaxItems(10)
			);

		await Run(schema);
	}

	[Test]
	public async Task ItemsAreNotString()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.MinItems(1)
			.Not(new JsonSchemaBuilder()
				.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			);

		await Run(schema);
	}

	[Test]
	[Ignore("flaky, not sure why")]
	public async Task DoesNotContainString()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Not(new JsonSchemaBuilder()
				.Contains(new JsonSchemaBuilder().Type(SchemaValueType.String))
			);

		await Run(schema);
	}

	[Test]
	public async Task ContainsAtLeastOneNonstring()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Contains(new JsonSchemaBuilder()
				.Not(new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
				)
			);

		await Run(schema);
	}

	[Test]
	[Ignore("flaky, not sure why")]
	public async Task DoesNotContainStringOrNull()
	{
		JsonSchema schema = new JsonSchemaBuilder()
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

		await Run(schema);
	}

	[Test]
	public async Task AnObjectThatDoesNotContainAFooPropertyWithoutSpecifyingType()
	{
		var schema = new JsonSchemaBuilder()
			.Not(new JsonSchemaBuilder().Required("foo"));

		await Run(schema, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });
	}
}