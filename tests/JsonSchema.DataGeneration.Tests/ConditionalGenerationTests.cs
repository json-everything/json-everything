using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

public class ConditionalGenerationTests
{
	[Test]
	public void IfThenElse()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.If(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Then(new JsonSchemaBuilder().MultipleOf(3))
			.Else(new JsonSchemaBuilder().Type(SchemaValueType.String));

		Run(schema);
	}

	[Test]
	public void ThenElse()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Boolean)
			.Then(new JsonSchemaBuilder().MultipleOf(3))
			.Else(new JsonSchemaBuilder().Type(SchemaValueType.String));

		Run(schema);
	}

	[Test]
	public void IfThen()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.If(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Then(new JsonSchemaBuilder().MultipleOf(3));

		Run(schema);
	}

	[Test]
	public void IfThen_WithGuaranteedIfCondition()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer)
			.If(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Then(new JsonSchemaBuilder().MultipleOf(3));

		Run(schema);
	}

	[Test]
	public void IfElse()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.If(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Else(new JsonSchemaBuilder().Type(SchemaValueType.String));

		Run(schema);
	}

	[Test]
	public void ConstInConditional()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.If(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Then(new JsonSchemaBuilder().Const("foo"))
			.Else(new JsonSchemaBuilder().Type(SchemaValueType.String));

		Run(schema);
	}

	[Test]
	public void TypeInConditionalResult()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.If(new JsonSchemaBuilder().Required("foo"))
			.Then(new JsonSchemaBuilder().Type(SchemaValueType.Object))
			.Else(true);

		Run(schema);
	}

	[Test]
	public void TypeInConditionalResult2()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
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

		Run(schema);
	}

	[Test]
	public void AllOfWithMutuallyExclusiveConditionals()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("kind", new JsonSchemaBuilder().Type(SchemaValueType.String).Enum("A", "B")),
				("value", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.Required("kind", "value")
			.AllOf(
				new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder().Properties(("kind", new JsonSchemaBuilder().Const("A"))).Required("kind"))
					.Then(new JsonSchemaBuilder().Properties(("value", new JsonSchemaBuilder().Const(1))).Required("value")),
				new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder().Properties(("kind", new JsonSchemaBuilder().Const("B"))).Required("kind"))
					.Then(new JsonSchemaBuilder().Properties(("value", new JsonSchemaBuilder().Const(2))).Required("value"))
			);

		Run(schema);
	}
}