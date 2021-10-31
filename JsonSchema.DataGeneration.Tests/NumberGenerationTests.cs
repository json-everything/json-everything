using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class NumberGenerationTests
	{
		[Test]
		public void GenerateNumber()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Number);

			Run(schema);
		}

		[Test]
		public void Minimum()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Number)
				.Minimum(0.2m);

			Run(schema);
		}

		[Test]
		public void Maximum()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Number)
				.Maximum(0.8m);

			Run(schema);
		}

		[Test]
		public void MultipleOf()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Number)
				.MultipleOf(0.3m);

			Run(schema);
		}

		[Test]
		public void NotMultipleOf()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Number)
				.Not(new JsonSchemaBuilder().MultipleOf(0.6m));

			Run(schema);
		}

		[Test]
		public void MultipleOfAndNotMultipleOf()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Number)
				.MultipleOf(0.3m)
				.Not(new JsonSchemaBuilder().MultipleOf(0.6m));

			Run(schema);
		}
	}
}
