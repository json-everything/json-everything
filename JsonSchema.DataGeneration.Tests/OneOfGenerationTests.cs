using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

internal class OneOfGenerationTests
{
	[Test]
	public async Task OneOfWithDifferentTypesButOneTypeIsDuplicatedForcingTheOther()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.OneOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Integer),
				new JsonSchemaBuilder().Type(SchemaValueType.Integer),
				new JsonSchemaBuilder().Type(SchemaValueType.String)
			);

		await Run(schema);
	}

	[Test]
	public async Task OneOfWithMultipleOfCannotBeMultipleOfBoth()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer)
			.OneOf(
				new JsonSchemaBuilder()
					.MultipleOf(2),
				new JsonSchemaBuilder()
					.MultipleOf(3)
			)
			.Minimum(0)
			.Maximum(50);

		await Run(schema);
	}
}