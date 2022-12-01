using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class NullTests
{
	private static readonly JsonSchema _schema =
		new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Required("foo");

	[Test]
	public void PropertyWithNullValuePasses()
	{
		var json = JsonNode.Parse("{\"foo\": null}");

		var result = _schema.Evaluate(json, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
	}

	[Test]
	public void MissingPropertyFails()
	{
		var json = JsonNode.Parse("{}");

		var result = _schema.Evaluate(json, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertInvalid();
	}
}