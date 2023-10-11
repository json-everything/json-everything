using System.Linq;
using Json.Schema.Analysis;
using NUnit.Framework;

namespace Json.Schema.Tests.Analysis;

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