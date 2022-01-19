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
	}
}