using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class NullTests
{
	private static readonly JsonSchema _schema =
		new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Required("foo");

	[Test]
	public async Task PropertyWithNullValuePasses()
	{
		var json = JsonNode.Parse("{\"foo\": null}");

		var result = await _schema.Evaluate(json, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
	}

	[Test]
	public async Task MissingPropertyFails()
	{
		var json = JsonNode.Parse("{}");

		var result = await _schema.Evaluate(json, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertInvalid();
	}
}