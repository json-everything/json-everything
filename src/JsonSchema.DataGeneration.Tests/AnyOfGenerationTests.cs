using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

internal class AnyOfGenerationTests
{
	[Test]
	public void AnyOfWithDifferentTypes()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AnyOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Number),
				new JsonSchemaBuilder().Type(SchemaValueType.String)
			);

		Run(schema, buildOptions);
	}

	[Test]
	public void AnyOfWithImpossibleFirstItem()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AnyOf(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.Number)
					.Not(new JsonSchemaBuilder()
						.Type(SchemaValueType.Number)),
				new JsonSchemaBuilder().Type(SchemaValueType.String)
			);

		Run(schema, buildOptions);
	}
}