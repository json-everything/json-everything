using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class NotGenerationTests
	{
		[Test]
		public void NotAnObject()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Not(new JsonSchemaBuilder().Type(SchemaValueType.Object));

			Run(schema);
		}

		[Test]
		public void DefinitelyAString()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Not(new JsonSchemaBuilder()
					.Type(SchemaValueType.Object |
					      SchemaValueType.Array |
					      SchemaValueType.Integer |
					      SchemaValueType.Number |
					      SchemaValueType.Null |
					      SchemaValueType.Boolean)
				);

			Run(schema);
		}

		[Test]
		public void NotInRange()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Not(new JsonSchemaBuilder()
					.Minimum(100)
					.Maximum(500)
				);

			Run(schema);
		}

		[Test]
		public void NumberNotInSubrange()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(0)
				.Maximum(1000)
				.Not(new JsonSchemaBuilder()
					.Minimum(100)
					.Maximum(500)
				);

			Run(schema);
		}

		// TODO: verify that array generation is checking bound type for min/max items (and props, too)
		[Test]
		public void ItemCountNotInRange()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.MaxItems(20)
				.Not(new JsonSchemaBuilder()
					.MinItems(5)
					.MaxItems(10)
				);

			Run(schema);
		}

		[Test]
		public void ItemsAreNotString()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.MinItems(1)
				.Not(new JsonSchemaBuilder()
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
				);

			Run(schema);
		}

		[Test]
		[Ignore("flaky, not sure why")]
		public void DoesNotContainString()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.Not(new JsonSchemaBuilder()
					.Contains(new JsonSchemaBuilder().Type(SchemaValueType.String))
				);

			Run(schema);
		}

		[Test]
		public void ContainsAtLeastOneNonstring()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.Contains(new JsonSchemaBuilder()
					.Not(new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					)
				);

			Run(schema);
		}

		[Test]
		[Ignore("flaky, not sure why")]
		public void DoesNotContainStringOrNull()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.AllOf(
					new JsonSchemaBuilder()
						.Not(new JsonSchemaBuilder()
							.Contains(new JsonSchemaBuilder().Type(SchemaValueType.String)
							)
						),
					new JsonSchemaBuilder()
						.Not(new JsonSchemaBuilder()
							.Contains(new JsonSchemaBuilder().Type(SchemaValueType.Null)
							)
						)
				);

			Run(schema);
		}
	}
}