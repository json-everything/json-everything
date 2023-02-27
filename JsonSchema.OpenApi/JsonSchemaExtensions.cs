using System.Text.Json.Nodes;

namespace Json.Schema.OpenApi;

public static class JsonSchemaExtensions
{
	public static DiscriminatorKeyword? GetDiscriminator(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DiscriminatorKeyword>(DiscriminatorKeyword.Name, out var k) ? k! : null;
	}

	public static JsonNode? GetExample(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ExampleKeyword>(ExampleKeyword.Name, out var k) ? k!.Value : null;
	}

	public static string? GetExternalDocs(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ExternalDocsKeyword>(ExternalDocsKeyword.Name, out var k) ? k!.Description : null;
	}

	public static XmlKeyword? GetXml(this JsonSchema schema)
	{
		return schema.TryGetKeyword<XmlKeyword>(XmlKeyword._Name, out var k) ? k! : null;
	}
}