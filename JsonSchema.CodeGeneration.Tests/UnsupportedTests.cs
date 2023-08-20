namespace Json.Schema.CodeGeneration.Tests;

public class UnsupportedTests
{
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

	[Test]
	[Ignore("This doesn't really test multiple cases because the cases are currently mutually exclusive.")]
	// I edited the cases to get some overlap for this test, then I undid the edit
	public void MultipleMatches()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Title("Host")
			.Type(SchemaValueType.Object)
			// part of custom object, which disallows additionalProperties
			.Properties(
				("Foo", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			// part of dictionary, which disallows properties
			.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.Integer));

		VerifyFailure(schema);
	}
}