using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

internal class AnyOfGenerationTests
{
	[Test]
	public async Task AnyOfWithDifferentTypes()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.AnyOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Number),
				new JsonSchemaBuilder().Type(SchemaValueType.String)
			);

		await Run(schema);
	}

	[Test]
	public async Task AnyOfWithImpossibleFirstItem()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.AnyOf(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Number)
					.Not(new JsonSchemaBuilder()
						.Type(SchemaValueType.Number)),
				new JsonSchemaBuilder().Type(SchemaValueType.String)
			);

		await Run(schema);
	}
}