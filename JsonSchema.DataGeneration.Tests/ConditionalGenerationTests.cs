using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class ConditionalGenerationTests
	{
		[Test]
		public void IfThenElse()
		{
			var schema = new JsonSchemaBuilder()
				.If(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				.Then(new JsonSchemaBuilder().MultipleOf(3))
				.Else(new JsonSchemaBuilder().Type(SchemaValueType.String));

			Run(schema);
		}

		[Test]
		public void ThenElse()
		{
			var schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Boolean)
				.Then(new JsonSchemaBuilder().MultipleOf(3))
				.Else(new JsonSchemaBuilder().Type(SchemaValueType.String));

			Run(schema);
		}

		[Test]
		public void IfThen()
		{
			var schema = new JsonSchemaBuilder()
				.If(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				.Then(new JsonSchemaBuilder().MultipleOf(3));

			Run(schema);
		}

		[Test]
		public void IfElse()
		{
			var schema = new JsonSchemaBuilder()
				.If(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				.Else(new JsonSchemaBuilder().Type(SchemaValueType.String));

			Run(schema);
		}
	}
}
