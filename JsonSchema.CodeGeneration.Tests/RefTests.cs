namespace Json.Schema.CodeGeneration.Tests;

public class RefTests
{
	[Test]
	public void PropertyPointerRef()
	{
		var schema = new JsonSchemaBuilder()
			.Title("MyObject")
			.Defs(
				("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder().Ref("#/$defs/foo"))
			);
		var expected = @"public class MyObject
{
	public int Foo { get; set; }
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void PropertyAnchorRef()
	{
		var schema = new JsonSchemaBuilder()
			.Title("MyObject")
			.Defs(
				("foo", new JsonSchemaBuilder()
					.Anchor("foo")
					.Type(SchemaValueType.Integer)
				)
			)
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder().Ref("#foo"))
			);
		var expected = @"public class MyObject
{
	public int Foo { get; set; }
}
";

		VerifyCSharp(schema, expected);
	}

	[Test]
	public void PropertyExternalRef()
	{
		var schema = new JsonSchemaBuilder()
			.Title("MyObject")
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder().Ref("other"))
			);
		var other = new JsonSchemaBuilder()
			.Id("https://json-everything.net/other")
			.Type(SchemaValueType.Integer)
			.Build();

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(other);

		var expected = @"public class MyObject
{
	public int Foo { get; set; }
}
";

		VerifyCSharp(schema, expected, options);
	}
}