using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class IntegerGenerationTests
	{
		[Test]
		public void GenerateInteger()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer);

			Run(schema);
		}

		[Test]
		public void Minimum()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10);

			Run(schema);
		}

		[Test]
		public void Maximum()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Maximum(20);

			Run(schema);
		}

		[Test]
		public void MultipleOf()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.MultipleOf(20);

			Run(schema);
		}

		[Test]
		public void DecimalMultipleOf()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.MultipleOf(0.84m);

			Run(schema);
		}

		[Test]
		public void MultipleOfAndNotMultipleOf()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.MultipleOf(3)
				.Not(new JsonSchemaBuilder().MultipleOf(6));

			Run(schema);
		}
	}
}
