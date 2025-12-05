using System;
using Json.Schema.OpenApi.Keywords;

namespace Json.Schema.OpenApi;

/// <summary>
/// Declares the vocabularies of the supported drafts.
/// </summary>
public static class Vocabulary
{
	/// <summary>
	/// The OpenAPI 3.1 vocabulary ID.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static readonly Uri OpenApi_31Id = new("https://spec.openapis.org/oas/3.1/vocab/base");

	/// <summary>
	/// The OpenAPI 3.1 vocabulary.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static readonly Schema.Vocabulary OpenApi_31 =
		new(OpenApi_31Id,
			ExampleKeyword.Instance,
			DiscriminatorKeyword.Instance,
			ExternalDocsKeyword.Instance,
			XmlKeyword.Instance);
}
