using Json.Schema.CodeGeneration.Language;

namespace Json.Schema.CodeGeneration.Tests;

public class SimpleDeclarationTests
{
	[Test]
	public void String()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String);
		var code = schema.GenerateCode(CodeWriters.CSharp);

		Assert.AreEqual(string.Empty, code);
	}

	[Test]
	public void Enum()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Title("MyEnum")
			.Enum("Zero", "One", "Two");
		var code = schema.GenerateCode(CodeWriters.CSharp);
		var expected = @"public enum MyEnum
{
	Zero = 0,
	One = 1,
	Two = 2
}
";

		Assert.AreEqual(expected, code);
	}

	[Test]
	public void NamelessArray()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.String));
		var code = schema.GenerateCode(CodeWriters.CSharp);

		Assert.AreEqual(string.Empty, code);
	}

	[Test]
	public void NamedArray()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Title("MyArray")
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.String));
		var code = schema.GenerateCode(CodeWriters.CSharp);
		var expected = @"public class MyArray : List<string>
{
}
";

		Assert.AreEqual(expected, code);
	}

	[Test]
	public void Object()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Title("MyObject")
			.Type(SchemaValueType.Object)
			.Properties(
				("Alpha", new JsonSchemaBuilder().Type(SchemaValueType.Number)),
				("Beta", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("Gamma", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
			);
		var code = schema.GenerateCode(CodeWriters.CSharp);
		var expected = @"public class MyObject
{
	public double Alpha { get; set; }
	public int Beta { get; set; }
	public bool Gamma { get; set; }
}
";

		Assert.AreEqual(expected, code);
	}

	[Test]
	public void NamelessDictionary()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.AdditionalProperties(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer));
		var code = schema.GenerateCode(CodeWriters.CSharp);

		Assert.AreEqual(string.Empty, code);
	}

	[Test]
	public void NamedDictionary()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Title("MyDictionary")
			.Type(SchemaValueType.Object)
			.AdditionalProperties(new JsonSchemaBuilder()
				.Type(SchemaValueType.Number));
		var code = schema.GenerateCode(CodeWriters.CSharp);
		var expected = @"public class MyDictionary : Dictionary<string, double>
{
}
";

		Assert.AreEqual(expected, code);
	}
}