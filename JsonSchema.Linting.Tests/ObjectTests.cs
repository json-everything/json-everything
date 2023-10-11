namespace Json.Schema.Linting.Tests;

public class ObjectTests
{
	[Test]
	public void OnlyPropertyKeywords()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.MinProperties(5)
			.MaxProperties(10)
			.Properties(("foo", true))
			.PatternProperties(("^f[0-9]+$", true))
			.AdditionalProperties(true)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(0, diagnostics.Length);
	}

	[Test]
	public void NonPropertyKeywords()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.MinItems(5)
			.Items(true)
			.Contains(true)
			.MaxLength(6)
			.Pattern("^f[0-9]+$")
			.Minimum(5)
			.MultipleOf(28)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(7, diagnostics.Length);
	}

	[Test]
	public void AnnotationsIgnored()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Title("title")
			.Description("description")
			.Default(true)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(0, diagnostics.Length);
	}
}