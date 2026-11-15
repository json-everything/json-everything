#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class LearnJsonEverythingTestFixture
{
	public static IEnumerable<TestCaseData> TestCases
	{
		get
		{
			yield return new TestCaseData(JsonDocument.Parse("""{"instance": "foo","values": ["foo", "bar", "baz", "quux"],"isValid": true}""").RootElement);
			yield return new TestCaseData(JsonDocument.Parse("""{"instance": "foo","values": ["foo", "bar"],"isValid": false}""").RootElement);
		}
	}

	[TestCaseSource(nameof(TestCases))]
	[Ignore("Dev test for learning site")]
	public void RunCases(JsonElement data)
	{
		var result = Run(data);
		var expected = data.GetProperty("isValid").GetBoolean();
		Assert.That(result.IsValid, Is.EqualTo(expected));
	}

	private static EvaluationResults Run(JsonElement test)
	{
		var instance = test.GetProperty("instance");
		var values = test.GetProperty("values");

		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new(),
			Dialect = Dialect.Draft201909
		};

		var metaSchemaId = new Uri("https://learn.json-everything.net/schemas/meta-schema");
		JsonSchema metaSchema = new JsonSchemaBuilder(buildOptions)
			.Id(metaSchemaId)
			.Vocabulary(
				(Vocabulary.Draft202012_Core.Id, true),
				(Vocabulary.Draft202012_Applicator.Id, true),
				(Vocabulary.Draft202012_Validation.Id, true)
			)
			.Properties(
				("enum", new JsonSchemaBuilder().MinItems(3))
			);

		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(metaSchemaId)
			.Enum(values.EnumerateArray().Select(x => x.GetString()!));

		return schema.Evaluate(instance);
	}
}
#endif