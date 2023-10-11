namespace Json.Schema.Linting.Tests;

public class ContainsTests
{
	[Test]
	public void MaxContainsWithoutContains()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.MaxContains(5)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreNotEqual(0, diagnostics.Length);
	}

	[Test]
	public void MinContainsWithoutContains()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.MinContains(5)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreNotEqual(0, diagnostics.Length);
	}

	[Test]
	public void MaxContainsWithContains()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.MaxContains(5)
			.Contains(true)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(0, diagnostics.Length);
	}

	[Test]
	public void MinContainsWithContains()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.MinContains(5)
			.Contains(true)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(0, diagnostics.Length);
	}
}