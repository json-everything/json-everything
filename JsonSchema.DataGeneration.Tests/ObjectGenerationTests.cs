using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class ObjectGenerationTests
	{
		[Test]
		public void GeneratesSingleProperty()
		{
			var schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", true)
				);

			Run(schema);
		}

		[Test]
		public void GeneratesMultipleProperties()
		{
			var schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", true),
					("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				);

			Run(schema);
		}

		[Test]
		public void AdditionalPropertiesFalse()
		{
			var schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", true),
					("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				)
				.AdditionalProperties(false);

			Run(schema);
		}

		[Test]
		public void RequiredPropertyNotListedInProperties()
		{
			var schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", true),
					("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				)
				.Required("baz");

			Run(schema);
		}

		[Test]
		public void DefineThreePickTwo()
		{
			var schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", true),
					("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
					("baz", new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
				.MaxProperties(2);

			Run(schema);
		}

		[Test]
		public void DefineThreePickTwoButMustContainBaz()
		{
			var schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", true),
					("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
					("baz", new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
				.Required("baz")
				.MaxProperties(2);

			Run(schema);
		}
	}
}