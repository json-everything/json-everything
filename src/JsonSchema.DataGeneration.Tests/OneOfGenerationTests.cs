using NUnit.Framework;
using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

internal class OneOfGenerationTests
{
	[Test]
	public void OneOfWithDifferentTypesButOneTypeIsDuplicatedForcingTheOther()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.OneOf(
				new JsonSchemaBuilder().Type(SchemaValueType.Integer),
				new JsonSchemaBuilder().Type(SchemaValueType.Integer),
				new JsonSchemaBuilder().Type(SchemaValueType.String)
			);

		Run(schema, buildOptions);
	}

	[Test]
	public void OneOfWithMultipleOfCannotBeMultipleOfBoth()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer)
			.OneOf(
				new JsonSchemaBuilder()
					.MultipleOf(2),
				new JsonSchemaBuilder()
					.MultipleOf(3)
			)
			.Minimum(0)
			.Maximum(50);

		Run(schema, buildOptions);
	}
}