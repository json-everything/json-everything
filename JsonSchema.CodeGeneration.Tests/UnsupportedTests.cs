namespace Json.Schema.CodeGeneration.Tests;

public class UnsupportedTests
{
	[Test]
	public void DifferentTypesWithSameName()
	{
		var schema = new JsonSchemaBuilder()
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
		var schema = new JsonSchemaBuilder()
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
		var schema = new JsonSchemaBuilder()
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
		var schema = new JsonSchemaBuilder()
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
		var schema = new JsonSchemaBuilder()
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
		var schema = new JsonSchemaBuilder()
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

	[Test]
	public void NamelessObject()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Alpha", new JsonSchemaBuilder().Type(SchemaValueType.Number)),
				("Beta", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("Gamma", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
			);

		VerifyFailure(schema);
	}

	[Test]
	public void BadTypeName_Dot()
	{
		var schema = new JsonSchemaBuilder()
			.Title("Dots.do not work")
			.Enum("One", "Two", "Three");

		VerifyFailure(schema);
	}

	[Test]
	public void BadEnumValueNames()
	{
		var schema = new JsonSchemaBuilder()
			.Title("MyEnum")
			.Enum("On.e", "Tw/o", "Thr:ee");

		VerifyFailure(schema);
	}

	[Test]
	public void BadTypeName_Colon()
	{
		var schema = new JsonSchemaBuilder()
			.Title("Colons:do not work")
			.Enum("One", "Two", "Three");

		VerifyFailure(schema);
	}

	[Test]
	public void BadTypeName_Slash()
	{
		var schema = new JsonSchemaBuilder()
			.Title("Slashes/do not work")
			.Enum("One", "Two", "Three");

		VerifyFailure(schema);
	}

	[Test]
	public void BadTypeName_Backslash()
	{
		var schema = new JsonSchemaBuilder()
			.Title("Backslashes\\do not work")
			.Enum("One", "Two", "Three");

		VerifyFailure(schema);
	}

	[Test]
	public void BadTypeName_Newline()
	{
		var schema = new JsonSchemaBuilder()
			.Title("Newlines\ndo not work")
			.Enum("One", "Two", "Three");

		VerifyFailure(schema);
	}

	[Test]
	public void BadObjectPropertyNames()
	{
		var schema = new JsonSchemaBuilder()
			.Title("MyObject")
			.Type(SchemaValueType.Object)
			.Properties(
				("Alp.ha", new JsonSchemaBuilder().Type(SchemaValueType.Number)),
				("Be\ta", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("Ga:mma", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
			);

		VerifyFailure(schema);
	}
}