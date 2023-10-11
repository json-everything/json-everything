namespace Json.Schema.Linting.Tests;

public class EnumTests
{
	[Test]
	public void AnnotationsIgnored()
	{
		var schema = new JsonSchemaBuilder()
			.Enum(14, "string", true)
			.Title("title")
			.Description("description")
			.Default(true)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(0, diagnostics.Length);
	}

	[Test]
	public void NumberKeywordsUnnecessary()
	{
		var schema = new JsonSchemaBuilder()
			.Enum(14, "string", true)
			.Minimum(5)
			.Maximum(20)
			.ExclusiveMinimum(4)
			.ExclusiveMaximum(21)
			.MultipleOf(8)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(5, diagnostics.Length);
	}
}