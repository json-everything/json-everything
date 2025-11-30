using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class SlackTests
{
	[Test]
	public void TypeNonNullAndNullFailsValidation()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("test", new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder()
						.Type(SchemaValueType.String, SchemaValueType.Null)
					)
				)
			)
			.Required("test");

		var instance = JsonDocument.Parse("""
			{
				"test": {
					"a": "aaa",
					"b": null
				}
			}
			""").RootElement;

		var results = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		results.AssertValid();
	}
}