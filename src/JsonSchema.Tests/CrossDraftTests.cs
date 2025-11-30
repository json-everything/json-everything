using System.Collections.Generic;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class CrossDraftTests
{
	public static IEnumerable<TestCaseData> ArrayItemsIsAllowedForDraft7Cases
	{
		get
		{
			yield return new TestCaseData(JsonDocument.Parse("""{ "foo": ["string", 42] }""").RootElement, true);
			yield return new TestCaseData(JsonDocument.Parse("""{ "foo": ["string", "other string"] }""").RootElement, false);
		}
	}

	[TestCaseSource(nameof(ArrayItemsIsAllowedForDraft7Cases))]
	public void ArrayItemsIsAllowedForDraft7(JsonElement instance, bool valid)
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new()
		};

		var schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.Draft202012Id)
			.Id("base")
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder().Ref("foo"))
			)
			.Defs(
				("foo-def", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft7Id)
					.Id("foo")
					.Type(SchemaValueType.Array)
					.Items([
						new JsonSchemaBuilder().Type(SchemaValueType.String),
						new JsonSchemaBuilder().Type(SchemaValueType.Integer)
					])
				)
			)
			.Build();

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};

		var result = schema.Evaluate(instance, options);

		if (valid)
			result.AssertValid();
		else
			result.AssertInvalid();
	}

	[TestCase("http://json-schema.org/draft-06/schema#")]
	[TestCase("http://json-schema.org/draft-07/schema#")]
	public void ContainsShouldIgnoreMinContainsForEarlierDrafts(string metaSchemaId)
	{
		var dialect = metaSchemaId switch
		{
			"http://json-schema.org/draft-06/schema#" => Dialect.Draft06,
			"http://json-schema.org/draft-07/schema#" => Dialect.Draft07,
			_ => throw new System.ArgumentOutOfRangeException(nameof(metaSchemaId), metaSchemaId, null)
		};
		var buildOptions = new BuildOptions { Dialect = dialect };

		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
			)
			.Contains(new JsonSchemaBuilder().Const(4))
			// Introduced with Draft 2019-09
			.MinContains(2);

		var instance = JsonDocument.Parse("[2, 3, 4, 5]").RootElement;

		var results = schema.Evaluate(instance);

		results.AssertValid();
	}
}