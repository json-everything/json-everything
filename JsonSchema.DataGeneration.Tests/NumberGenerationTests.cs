using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

public class NumberGenerationTests
{
	[Test]
	public async Task GenerateNumber()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number);

		await Run(schema);
	}

	[Test]
	public async Task Minimum()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(0.2m);

		await Run(schema);
	}

	[Test]
	public async Task Maximum()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Maximum(0.8m);

		await Run(schema);
	}

	[Test]
	public async Task MultipleOf()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.MultipleOf(0.3m);

		await Run(schema);
	}

	[Test]
	public async Task NotMultipleOf()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Not(new JsonSchemaBuilder().MultipleOf(0.6m));

		await Run(schema);
	}

	[Test]
	public async Task MultipleOfAndNotMultipleOf()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.MultipleOf(0.3m)
			.Not(new JsonSchemaBuilder().MultipleOf(0.6m));

		await Run(schema);
	}

	[Test]
	[Ignore("flaky")]
	public async Task MultipleOfWithRange()
	{
		// This test highlights an issue with the number generation:
		// Periodically, the rounding can cause the generation to be
		// out of the range.  Generation is tried 5 times per range.
		// If it fails all 5 times, this test will fail.
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(10)
			.Maximum(20)
			.MultipleOf(2.3m);

		await Run(schema);
	}
}