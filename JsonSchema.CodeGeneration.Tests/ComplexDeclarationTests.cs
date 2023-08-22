namespace Json.Schema.CodeGeneration.Tests;

public class ComplexDeclarationTests
{
	[Test]
	public void ObjectNestedInObjectIsAlsoWritten()
	{
		var schema = new JsonSchemaBuilder()
			.Title("Complex")
			.Type(SchemaValueType.Object)
			.Properties(
				("ObjectDictionary", new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder()
						.Title("FooBar")
						.Type(SchemaValueType.Object)
						.Properties(
							("Foo", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
							("Bar", new JsonSchemaBuilder().Type(SchemaValueType.String))
						)
					)
				)
			);

		var expected = @"public class Complex
{
	public Dictionary<string, FooBar> ObjectDictionary { get; set; }
}
public class FooBar
{
	public bool Foo { get; set; }
	public string Bar { get; set; }
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void DuplicateObjectIsNotWrittenTwice()
	{
		var schema = new JsonSchemaBuilder()
			.Title("Complex")
			.Type(SchemaValueType.Object)
			.Properties(
				("ObjectDictionary", new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder()
						.Title("FooBar")
						.Type(SchemaValueType.Object)
						.Properties(
							("Foo", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
							("Bar", new JsonSchemaBuilder().Type(SchemaValueType.String))
						)
					)
				),
				("SingleObject", new JsonSchemaBuilder()
					.Title("FooBar")
					.Type(SchemaValueType.Object)
					.Properties(
						("Foo", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
						("Bar", new JsonSchemaBuilder().Type(SchemaValueType.String))
					)
				)
			);

		var expected = @"public class Complex
{
	public Dictionary<string, FooBar> ObjectDictionary { get; set; }
	public FooBar SingleObject { get; set; }
}
public class FooBar
{
	public bool Foo { get; set; }
	public string Bar { get; set; }
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void StringSchemaAllowsOtherKeywords()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.MinLength(42);

		VerifyCSharp(schema, string.Empty);
	}
}