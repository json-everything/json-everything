using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class ArrayGenerationTests
	{
		[Test]
		public void GenerateArrayOfNumbers()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder()
					.Type(SchemaValueType.Number)
					.Minimum(10)
					.Maximum(20)
					.MultipleOf(2.3m))
				.MinItems(3)
				.MaxItems(10);

			Run(schema);
		}

		[Test]
		public void GenerateArrayOfNumbersNoMax()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder()
					.Type(SchemaValueType.Number)
					.Minimum(10)
					.Maximum(20)
					.MultipleOf(2.3m))
				.MinItems(3);

			Run(schema);
		}

		[Test]
		public void GenerateArrayOfNumbersWithAllOf()
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
					.Maximum(50))
				.MinItems(3)
				.MaxItems(10);

			Run(schema);
		}
	}
}
