namespace Json.Schema.Linting.Tests;

public class MinMaxTests
{
	[Test]
	public void MinimumAndExclusiveMinimum()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(10)
			.ExclusiveMinimum(20)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(1, diagnostics.Length);
	}

	[Test]
	public void MaximumAndExclusiveMaximum()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Maximum(10)
			.ExclusiveMaximum(20)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(1, diagnostics.Length);
	}

	[Test]
	public void MinimumLessThanMaximum()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(10)
			.Maximum(20)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(0, diagnostics.Length);
	}

	[Test]
	public void MaximumLessThanMinimum()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(20)
			.Maximum(10)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(1, diagnostics.Length);
	}

	[Test]
	public void MinimumEqualToMaximum()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(10)
			.Maximum(10)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(0, diagnostics.Length);
	}

	[Test]
	public void MinimumLessThanExclusiveMaximum()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(10)
			.ExclusiveMaximum(20)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(0, diagnostics.Length);
	}

	[Test]
	public void ExclusiveMaximumLessThanMinimum()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(20)
			.ExclusiveMaximum(10)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(1, diagnostics.Length);
	}

	[Test]
	public void MinimumEqualToExclusiveMaximum()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(10)
			.ExclusiveMaximum(10)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(1, diagnostics.Length);
	}

}