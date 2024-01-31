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

		var code = VerifyCSharp(schema, expected);

		var json = @"{
  ""ObjectDictionary"": {
    ""Alpha"": {
      ""Foo"": false,
      ""Bar"": ""a string""
    },
    ""Beta"": {
      ""Foo"": true,
      ""Bar"": ""another string""
    },
    ""Gamma"": {
      ""Foo"": false,
      ""Bar"": ""a different string""
    }
  }
}";

		VerifyDeserialization(code, json, isReflectionAllowed: true);
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

		var code = VerifyCSharp(schema, expected);

		var json = @"{
  ""ObjectDictionary"": {
    ""Alpha"": {
      ""Foo"": false,
      ""Bar"": ""a string""
    },
    ""Beta"": {
      ""Foo"": true,
      ""Bar"": ""another string""
    },
    ""Gamma"": {
      ""Foo"": false,
      ""Bar"": ""a different string""
    }
  },
  ""SingleObject"": {
    ""Foo"": true,
    ""Bar"": ""a single object""
  }
}";

		VerifyDeserialization(code, json, isReflectionAllowed: true);
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