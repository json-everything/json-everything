using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

public class ObjectGenerationTests
{
	[Test]
	public async Task GeneratesSingleProperty()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true)
			);

		await Run(schema);
	}

	[Test]
	public async Task GeneratesMultipleProperties()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true),
				("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			);

		await Run(schema);
	}

	[Test]
	public async Task AdditionalPropertiesFalse()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true),
				("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.AdditionalProperties(false);

		await Run(schema);
	}

	[Test]
	public async Task RequiredPropertyNotListedInProperties()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true),
				("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.Required("baz");

		await Run(schema);
	}

	[Test]
	public async Task DefineThreePickTwo()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true),
				("bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("baz", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.MaxProperties(2);

		await Run(schema);
	}

	[Test]
	public async Task DefineThreePickTwoButMustContainBaz()
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

		await Run(schema);
	}
}