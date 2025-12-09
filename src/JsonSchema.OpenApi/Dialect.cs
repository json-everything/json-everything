using Json.Schema.OpenApi.Keywords;

namespace Json.Schema.OpenApi;

/// <summary>
/// Provides dialect definitions for OpenAPI 3.1 JSON Schema extensions.
/// </summary>
/// <remarks>This class exposes constants and dialect configurations for the OpenAPI 3.1 dialect.
/// It is intended for use when working with JSON Schema implementations that require OpenAPI vocabulary
/// extensions. All members are static and thread-safe.</remarks>
public static class Dialect
{
	/// <summary>
	/// Provides the JSON Schema dialect definition for OpenAPI 3.1 with support for the OpenAPI-specific keywords.
	/// </summary>
	/// <remarks>This dialect enables usage of the 'example', 'discriminator', 'externalDocs', and 'xml' keywords
	/// as defined in the OpenAPI 3.1 specification. Use this dialect when validating schemas that require these keywords.
	/// The dialect is configured with the appropriate meta-schema identifier and keyword support.</remarks>
	// ReSharper disable once InconsistentNaming
	public static readonly Schema.Dialect OpenApi_31 =
		Schema.Dialect.Draft202012.With([
				ExampleKeyword.Instance,
				DiscriminatorKeyword.Instance,
				ExternalDocsKeyword.Instance,
				XmlKeyword.Instance
			],
			MetaSchemas.OpenApi_31Id,
			false,
			true);
}
