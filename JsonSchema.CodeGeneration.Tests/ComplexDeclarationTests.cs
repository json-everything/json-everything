using Json.Schema.CodeGeneration.Language;

namespace Json.Schema.CodeGeneration.Tests;

public class ComplexDeclarationTests
{
	[Test]
	public void ObjectNestedInObjectIsAlsoWritten()
	{
		JsonSchema schema = new JsonSchemaBuilder()
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

		var code = schema.GenerateCode(CodeWriters.CSharp);

		Console.WriteLine(code);
	}

	[Test]
	public void DuplicateObjectIsNotWrittenTwice()
	{
		JsonSchema schema = new JsonSchemaBuilder()
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

		var code = schema.GenerateCode(CodeWriters.CSharp);

		Console.WriteLine(code);
	}

	[Test]
	public void StringSchemaAllowsOtherKeywords()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.MinLength(42);

		var code = schema.GenerateCode(CodeWriters.CSharp);

		Console.WriteLine(code);
		Assert.AreEqual(string.Empty, code);
	}
}