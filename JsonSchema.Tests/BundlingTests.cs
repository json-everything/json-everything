using System;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class BundlingTests
	{
		[Test]
		public void Draft202012ContainsDraft7_InnerShouldIgnore202012Keywords()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft202012Id)
				.Id("https://json-everything/draft202012schema")
				.Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder().Ref("#/$defs/draft7schema"))
				.Defs(
					("draft7schema", new JsonSchemaBuilder()
						.Schema(MetaSchemas.Draft7Id)
						.Id("https://json-everything/draft7schema")
						.Type(SchemaValueType.Array)
						// this should be ignored since it's a draft 2020-12 keyword in a draft 7 schema
						.PrefixItems(new JsonSchemaBuilder().Type(SchemaValueType.Number))
						.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
					)
				);

			var instance = JsonDocument.Parse("[[\"string\"]]");

			var result = schema.Validate(instance.RootElement, new ValidationOptions() {OutputFormat = OutputFormat.Detailed});

			Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions {WriteIndented = true}));
			Assert.True(result.IsValid);
		}
	}
}
