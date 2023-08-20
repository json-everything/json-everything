namespace Json.Schema.CodeGeneration.Tests;

public class SimpleDeclarationTests
{
	[Test]
	public void String()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String);

		VerifyCSharp(schema, string.Empty);
	}

	[Test]
	public void NamedStringIgnoresName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("NamedString")
			.Type(SchemaValueType.String);

		VerifyCSharp(schema, string.Empty);
	}

	[Test]
	public void Integer()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer);

		VerifyCSharp(schema, string.Empty);
	}

	[Test]
	public void NamedIntegerIgnoresName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("NamedInteger")
			.Type(SchemaValueType.Integer);

		VerifyCSharp(schema, string.Empty);
	}

	[Test]
	public void Number()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number);

		VerifyCSharp(schema, string.Empty);
	}

	[Test]
	public void NamedNumberIgnoresName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("NamedNumber")
			.Type(SchemaValueType.Number);

		VerifyCSharp(schema, string.Empty);
	}

	[Test]
	public void Boolean()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Boolean);

		VerifyCSharp(schema, string.Empty);
	}

	[Test]
	public void NamedBooleanIgnoresName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("NamedBoolean")
			.Type(SchemaValueType.Boolean);

		VerifyCSharp(schema, string.Empty);
	}

	[Test]
	public void Enum()
	{
		var schema = new JsonSchemaBuilder()
			.Title("MyEnum")
			.Enum("Zero", "One", "Two");
		var expected = @"public enum MyEnum
{
	Zero = 0,
	One = 1,
	Two = 2
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void NamelessArray()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.String));

		VerifyCSharp(schema, string.Empty);
	}

	[Test]
	public void NamedArray()
	{
		var schema = new JsonSchemaBuilder()
			.Title("MyArray")
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.String));
		var expected = @"public class MyArray : List<string>
{
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void Object()
	{
		var schema = new JsonSchemaBuilder()
			.Title("MyObject")
			.Type(SchemaValueType.Object)
			.Properties(
				("Alpha", new JsonSchemaBuilder().Type(SchemaValueType.Number)),
				("Beta", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("Gamma", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
			);
		var expected = @"public class MyObject
{
	public double Alpha { get; set; }
	public int Beta { get; set; }
	public bool Gamma { get; set; }
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void NamelessDictionary()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.AdditionalProperties(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer));

		VerifyCSharp(schema, string.Empty);
	}

	[Test]
	public void NamedDictionary()
	{
		var schema = new JsonSchemaBuilder()
			.Title("MyDictionary")
			.Type(SchemaValueType.Object)
			.AdditionalProperties(new JsonSchemaBuilder()
				.Type(SchemaValueType.Number));
		var expected = @"public class MyDictionary : Dictionary<string, double>
{
}
";

		VerifyCSharp(schema, expected);
	}
}