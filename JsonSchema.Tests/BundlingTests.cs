using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class BundlingTests
{
	[Test]
	public void Draft202012ContainsDraft7_InnerShouldIgnore202012Keywords()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft201909Id)
			.Id("https://json-everything/draft2019schema")
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Ref("#/$defs/draft2020schema"))
			.Defs(
				("draft2020schema", new JsonSchemaBuilder()
					.Schema(MetaSchemas.Draft202012Id)
					.Id("https://json-everything/draft2020schema")
					.Type(SchemaValueType.Array)
					// this should be processed even though the outer schema is draft 2019-09
					.PrefixItems(new JsonSchemaBuilder().Type(SchemaValueType.Number))
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
			);

		var instance = JsonDocument.Parse("[[1, \"other string\"]]");

		var result = schema.Validate(instance.RootElement, new ValidationOptions { OutputFormat = OutputFormat.Hierarchical });

		Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
		Assert.True(result.IsValid);
	}
}