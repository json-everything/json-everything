using System.Collections.Generic;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class CrossDraftTests
{
	public static IEnumerable<TestCaseData> ArrayItemsIsAllowedForDraft7Cases
	{
		get
		{
			yield return new TestCaseData((JsonNode)new JsonObject
			{
				["foo"] = new JsonArray { "string", 42 }
			}, true);
			yield return new TestCaseData((JsonNode)new JsonObject
			{
				["foo"] = new JsonArray { "string", "other string" }
			}, false);
		}
	}

	[TestCaseSource(nameof(ArrayItemsIsAllowedForDraft7Cases))]
	public void ArrayItemsIsAllowedForDraft7(JsonNode instance, bool valid)
	{
		var schema = new JsonSchemaBuilder()
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
					.Items(new JsonSchema[]
					{
						new JsonSchemaBuilder().Type(SchemaValueType.String),
						new JsonSchemaBuilder().Type(SchemaValueType.Integer)
					})
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
}