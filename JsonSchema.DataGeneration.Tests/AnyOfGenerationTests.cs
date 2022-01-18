using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	internal class AnyOfGenerationTests
	{
		[Test]
		public void AnyOfWithDifferentTypes()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.AnyOf(
					new JsonSchemaBuilder().Type(SchemaValueType.Number),
					new JsonSchemaBuilder().Type(SchemaValueType.String)
				);

			Run(schema);
		}

		[Test]
		public void AnyOfWithImpossibleFirstItem()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.AnyOf(
					new JsonSchemaBuilder()
						.Type(SchemaValueType.Number)
						.Not(new JsonSchemaBuilder()
							.Type(SchemaValueType.Number)),
					new JsonSchemaBuilder().Type(SchemaValueType.String)
				);

			Run(schema);
		}
	}
}
