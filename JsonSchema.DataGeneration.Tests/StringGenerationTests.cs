using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class StringGenerationTests
	{
		[Test]
		public void SimpleString()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String);

			Run(schema);
		}

		[Test]
		public void MinLength()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.MinLength(30);

			Run(schema);
		}

		[Test]
		public void MaxLength()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		public void SpecifiedRange()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		public void ContainsDog()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Pattern("dog");

			Run(schema);
		}

		[Test]
		public void ContainsDogWithSizeConstraints()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Pattern("dog")
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		public void DoesNotContainDog()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Not(new JsonSchemaBuilder().Pattern("dog"));

			Run(schema);
		}

		[Test]
		public void DoesNotContainDogWithSizeConstraints()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Not(new JsonSchemaBuilder().Pattern("dog"))
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		public void ContainsCatAndDoesNotContainDogWithSizeConstraints()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Not(new JsonSchemaBuilder().Pattern("dog"))
				.Pattern("cat")
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		public void ContainsEitherCatOrDog()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.AnyOf(
					new JsonSchemaBuilder().Pattern("dog"),
					new JsonSchemaBuilder().Pattern("cat")
				)
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}

		[Test]
		public void ContainsExclusivelyEitherCatOrDog()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.OneOf(
					new JsonSchemaBuilder().Pattern("dog"),
					new JsonSchemaBuilder().Pattern("cat")
				)
				.MinLength(10)
				.MaxLength(20);

			Run(schema);
		}
	}
}