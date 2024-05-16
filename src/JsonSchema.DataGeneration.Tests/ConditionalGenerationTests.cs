using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

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

	[Test]
	public void ConstInConditional()
	{
		var schema = new JsonSchemaBuilder()
			.If(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Then(new JsonSchemaBuilder().Const("foo"))
			.Else(new JsonSchemaBuilder().Type(SchemaValueType.String));

		Run(schema);
	}

	[Test]
	public void TypeInConditionalResult()
	{
		var schema = new JsonSchemaBuilder()
			.If(new JsonSchemaBuilder().Required("foo"))
			.Then(new JsonSchemaBuilder().Type(SchemaValueType.Object))
			.Else(true);

		Run(schema, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });
	}

	[Test]
	public void TypeInConditionalResult2()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("people", new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder()
						.If(new JsonSchemaBuilder()
							.Properties(
								("id", new JsonSchemaBuilder().Const("uLqeBv"))
							)
							.Required("id")
						)
						.Then(new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.Properties(
								("id", new JsonSchemaBuilder().Const("uLqeBv")),
								("relationshipStatus", new JsonSchemaBuilder().Const("married"))
							)
							.Required("id", "relationshipStatus")
						)
					)
				)
			)
			.Required("people");

		Run(schema, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });
	}
}