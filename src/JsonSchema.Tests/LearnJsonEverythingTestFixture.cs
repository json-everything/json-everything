using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class LearnJsonEverythingTestFixture
{
	public static IEnumerable<TestCaseData> TestCases
	{
		get
		{
			yield return new TestCaseData(JsonNode.Parse("{\r\n  \"instance\": \"foo\",\r\n  \"values\": [\"foo\", \"bar\", \"baz\", \"quux\"],\r\n  \"isValid\": true\r\n}"));
			yield return new TestCaseData(JsonNode.Parse("{\r\n  \"instance\": \"foo\",\r\n  \"values\": [\"foo\", \"bar\"],\r\n  \"isValid\": false\r\n}"));
		}
	}

	[TestCaseSource(nameof(TestCases))]
	[Ignore("Dev purposes only, supports learning site")]
	public void RunCases(JsonNode data)
	{
		var result = Run(data.AsObject());
		var expected = data["isValid"]!.GetValue<bool>();
		Assert.That(result.IsValid, Is.EqualTo(expected));
	}

	private static EvaluationResults Run(JsonObject test)
	{
		var instance = test["instance"];
		var values = test["values"]!;

		var metaSchemaId = new Uri("https://learn.json-everything.net/schemas/meta-schema");
		JsonSchema metaSchema = new JsonSchemaBuilder()
			.Id(metaSchemaId)
			.Properties(
				("enum", new JsonSchemaBuilder().MinItems(3))
			);

		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(metaSchemaId)
			.Enum((IEnumerable<JsonNode?>)values);

		var options = new EvaluationOptions
		{
			ValidateAgainstMetaSchema = true
		};

		options.SchemaRegistry.Register(metaSchema);

		return schema.Evaluate(instance, options);
	}
}
