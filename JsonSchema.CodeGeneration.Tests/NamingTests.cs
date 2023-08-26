namespace Json.Schema.CodeGeneration.Tests;

public class NamingTests
{
	[Test]
	public void PropertyNames()
	{
		var schema = new JsonSchemaBuilder()
			.Title("MyObject")
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("foo-bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("bar_baz", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("just-a-letter", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			);
		var expected = @"public class MyObject
{
	[JsonProperty(""foo"")]
	public int Foo { get; set; }
	[JsonProperty(""foo-bar"")]
	public int FooBar { get; set; }
	[JsonProperty(""bar_baz"")]
	public int BarBaz { get; set; }
	[JsonProperty(""just-a-letter"")]
	public int JustALetter { get; set; }
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void KebabedObjectTypeName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("my-object")
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			);
		var expected = @"public class MyObject
{
	public int Foo { get; set; }
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void SnakedObjectTypeName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("my_object")
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			);
		var expected = @"public class MyObject
{
	public int Foo { get; set; }
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void SpacedObjectTypeName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("my object")
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			);
		var expected = @"public class MyObject
{
	public int Foo { get; set; }
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void KebabedNumberedObjectTypeName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("my-2-object-5")
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			);
		var expected = @"public class My2Object5
{
	public int Foo { get; set; }
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void EnumValues()
	{
		var schema = new JsonSchemaBuilder()
			.Title("MyEnum")
			.Enum("one", "Two2", "Three-Third");
		var expected = @"public enum MyEnum
{
	One = 0,
	Two2 = 1,
	ThreeThird = 2
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void KebabedEnumTypeName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("my-enum")
			.Enum("One", "Two", "Three");
		var expected = @"public enum MyEnum
{
	One = 0,
	Two = 1,
	Three = 2
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void SnakedEnumTypeName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("my_enum")
			.Enum("One", "Two", "Three");
		var expected = @"public enum MyEnum
{
	One = 0,
	Two = 1,
	Three = 2
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void SpacedEnumTypeName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("my enum")
			.Enum("One", "Two", "Three");
		var expected = @"public enum MyEnum
{
	One = 0,
	Two = 1,
	Three = 2
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void KebabedNumberedEnumTypeName()
	{
		var schema = new JsonSchemaBuilder()
			.Title("my-2-object-5")
			.Enum("One", "Two", "Three");
		var expected = @"public enum My2Object5
{
	One = 0,
	Two = 1,
	Three = 2
}
";

		VerifyCSharp(schema, expected);
	}
}