using System.Linq;
using Json.Schema.Analysis;
using NUnit.Framework;

namespace Json.Schema.Tests.Analysis;

public class MissingTypeTests
{
	[Test]
	public void TypeMissing()
	{
		var schema = new JsonSchemaBuilder()
			.Properties(("foo", true))
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreNotEqual(0, diagnostics.Length);
	}

	[Test]
	public void TypeMissingInSubschema()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder()
					.Properties(("bar", true))
				)
			)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreNotEqual(0, diagnostics.Length);
	}

	[Test]
	public void TypeBoolean()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Boolean)
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(0, diagnostics.Length);
	}

	[Test]
	public void Empty()
	{
		var schema = new JsonSchemaBuilder()
			.Build();

		var diagnostics = schema.Analyze().ToArray();

		diagnostics.Output();
		Assert.AreEqual(0, diagnostics.Length);
	}

}