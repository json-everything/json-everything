using Json.Schema.CodeGeneration.Language;
using Json.Schema.CodeGeneration.Model;

namespace Json.Schema.CodeGeneration.Tests;

public class UnsupportedTests
{
	private static void VerifyFailure(JsonSchema schema)
	{
		var ex = Assert.Throws<SchemaConversionException>(() =>
		{
			var actual = schema.GenerateCode(CodeWriters.CSharp);

			Console.WriteLine(actual);
		});

		Console.WriteLine(ex);
	}

	[Test]
	public void DifferentTypesWithSameName()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Title("Host")
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder()
					.Title("Violation")
					.Enum("One", "Two", "Three")
				),
				("Bar", new JsonSchemaBuilder()
					.Title("Violation")
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					)
				)
			);

		VerifyFailure(schema);
	}

	[Test]
	public void DifferentButSimilarObjectTypesWithSameName()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Title("Host")
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder()
					.Title("Violation")
					.Type(SchemaValueType.Object)
					.Properties(
						("Baz", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
						)
					)
				),
				("Bar", new JsonSchemaBuilder()
					.Title("Violation")
					.Type(SchemaValueType.Object)
					.Properties(
						("Buz", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
						)
					)
				)
			);

		VerifyFailure(schema);
	}

	[Test]
	public void DifferentButSimilarEnumTypesWithSameName()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Title("Host")
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder()
					.Title("Violation")
					.Enum("One", "Two", "Three")
				),
				("Bar", new JsonSchemaBuilder()
					.Title("Violation")
					.Enum("One", "Three", "Two")
				)
			);

		VerifyFailure(schema);
	}

	[Test]
	public void DifferentButSimilarArrayTypesWithSameName()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Title("Host")
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder()
					.Title("Violation")
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
				),
				("Bar", new JsonSchemaBuilder()
					.Title("Violation")
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				)
			);

		VerifyFailure(schema);
	}

	[Test]
	public void DifferentButSimilarDictionaryTypesWithSameName()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Title("Host")
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder()
					.Title("Violation")
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.String))
				),
				("Bar", new JsonSchemaBuilder()
					.Title("Violation")
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				)
			);

		VerifyFailure(schema);
	}
}