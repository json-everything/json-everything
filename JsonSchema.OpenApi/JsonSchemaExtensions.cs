using System.Text.Json.Nodes;

namespace Json.Schema.OpenApi;

/// <summary>
/// Some extensions for <see cref="JsonSchema"/>
/// </summary>
public static class JsonSchemaExtensions
{
	/// <summary>
	/// Gets the `discriminator` keyword if it exists.
	/// </summary>
	public static DiscriminatorKeyword? GetDiscriminator(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DiscriminatorKeyword>(DiscriminatorKeyword.Name, out var k) ? k! : null;
	}

	/// <summary>
	/// Gets the value of `example` if the keyword exists.
	/// </summary>
	public static JsonNode? GetExample(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ExampleKeyword>(ExampleKeyword.Name, out var k) ? k!.Value : null;
	}

	/// <summary>
	/// Gets the value of `externalDocs` if the keyword exists.
	/// </summary>
	public static string? GetExternalDocs(this JsonSchema schema)
	{
		return schema.TryGetKeyword<ExternalDocsKeyword>(ExternalDocsKeyword.Name, out var k) ? k!.Description : null;
	}

	/// <summary>
	/// Gets the `xml` keyword if it exists.
	/// </summary>
	public static XmlKeyword? GetXml(this JsonSchema schema)
	{
		return schema.TryGetKeyword<XmlKeyword>(XmlKeyword._Name, out var k) ? k! : null;
	}
}