using System.Linq;
using Json.Schema.Analysis;
using NUnit.Framework;

namespace Json.Schema.Tests.Analysis;

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

public class ConstTests
{
	[Test]
	public void AnnotationsIgnored()
	{
		var schema = new JsonSchemaBuilder()
			.Const(14)
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
			.Const(14)
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