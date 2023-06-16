using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

public class IntegerGenerationTests
{
	[Test]
	public async Task GenerateInteger()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer);

		await Run(schema);
	}

	[Test]
	public async Task Minimum()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer)
			.Minimum(10);

		await Run(schema);
	}

	[Test]
	public async Task Maximum()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer)
			.Maximum(20);

		await Run(schema);
	}

	[Test]
	public async Task MultipleOf()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer)
			.MultipleOf(20);

		await Run(schema);
	}

	[Test]
	public async Task DecimalMultipleOf()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer)
			.MultipleOf(0.84m);

		await Run(schema);
	}

	[Test]
	public async Task MultipleOfAndNotMultipleOf()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer)
			.MultipleOf(3)
			.Not(new JsonSchemaBuilder().MultipleOf(6));

		await Run(schema);
	}
}