using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class BundlingTests
{
	[Test]
	public void Draft201909ContainsDraft202012_InnerShouldProcess202012Keywords()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft201909Id)
			.Id("https://json-everything/draft2019schema")
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Ref("draft2020schema"))
			.Defs(
				("draft2020schema", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft202012Id)
					.Id("https://json-everything/draft2020schema")
					.Type(SchemaValueType.Array)
					// this should be processed
					.PrefixItems(new JsonSchemaBuilder().Type(SchemaValueType.Number))
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
			);

		var instance = JsonDocument.Parse("[[1, \"other string\"]]");

		var result = schema.Evaluate(instance.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
	}

	[Test]
	public void Draft202012ContainsDraft201909_InnerShouldIgnore202012Keywords()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id("https://json-everything/draft2020schema")
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Ref("draft2019schema"))
			.Defs(
				("draft2019schema", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft201909Id)
					.Id("https://json-everything/draft2019schema")
					.Type(SchemaValueType.Array)
					// this should be not processed
					.PrefixItems(new JsonSchemaBuilder().Type(SchemaValueType.Number))
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
			);

		var instance = JsonDocument.Parse("[[\"one string\", \"other string\"]]");

		var result = schema.Evaluate(instance.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
	}
}