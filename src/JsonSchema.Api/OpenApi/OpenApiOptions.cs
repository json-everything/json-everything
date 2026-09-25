using System;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Configures the OpenAPI description and how it is published.
/// </summary>
/// <remarks>
/// Paths enable what they name: a description is served only when
/// <see cref="DocumentPath"/> has a value, and the reference page only when
/// <see cref="InteractivePath"/> does.  To describe an API without exposing it, leave both
/// unset, or set them only outside production.
/// </remarks>
public class OpenApiOptions
{
	/// <summary>
	/// Gets the description, assembled from the API surface.
	/// </summary>
	/// <remarks>
	/// Edit this to supply anything the analyzer cannot infer — contact details, servers,
	/// security schemes, descriptions.
	/// </remarks>
	public OpenApiDocument Document { get; }

	/// <summary>
	/// Gets or sets the path where the description is served.  Expressed without an extension.
	/// The extensions are specified <see cref="DocumentFormats"/>.  Defaults to `/openapi`.
	/// </summary>
	/// <remarks>
	/// Set to null to build the description without serving it.
	/// </remarks>
	public string? DocumentPath { get; set; } = "/openapi";

	/// <summary>
	/// Gets or sets the formats the description is served in.  Defaults to both.
	/// </summary>
	public OpenApiFormats DocumentFormats { get; set; } = OpenApiFormats.Json | OpenApiFormats.Yaml;

	/// <summary>
	/// Gets or sets the path the reference page is served from.  Defaults to
	/// `/openapi/reference`.
	/// </summary>
	/// <remarks>
	/// The page reads the description over HTTP, so <see cref="DocumentPath"/> must have a
	/// value and <see cref="DocumentFormats"/> must include
	/// <see cref="OpenApiFormats.Json"/>.  Set to null to serve no page.
	/// </remarks>
	public string? InteractivePath { get; set; } = "/openapi/reference";

	/// <summary>
	/// Gets or sets a stylesheet URL applied to the reference page after the built-in
	/// styles, so that its rules win where they overlap.
	/// </summary>
	/// <remarks>
	/// The built-in styles define their palette as custom properties on `:root` and give
	/// every element a class prefixed `oa-`, so a stylesheet can restyle the page either by
	/// redefining those properties or by targeting the classes directly.
	/// </remarks>
	public string? StylesheetUrl { get; set; }

	/// <summary>
	/// Gets or sets the file path the description is written to at startup.  Expressed
	/// without an extension, as <see cref="DocumentPath"/> is; one file is written per
	/// format in <see cref="DocumentFormats"/>.
	/// </summary>
	/// <remarks>
	/// Writing the description to disk lets it be committed, diffed, or handed to
	/// client-generation tooling.  Leave unset to write nothing.
	/// </remarks>
	public string? FileOutputPath { get; set; }

	/// <summary>
	/// Gets or sets whether request validation is registered alongside the description.
	/// Defaults to true.
	/// </summary>
	/// <remarks>
	/// The description states that a validated endpoint answers malformed input with
	/// `application/problem+json`, so registering validation here keeps the description
	/// honest without the consumer having to remember a second call.  Registration is
	/// idempotent, so calling <c>AddJsonSchemaValidation</c> explicitly still works, and the
	/// configuration passed there wins.  Set to false to describe an API whose validation is
	/// registered elsewhere, or not at all.
	/// </remarks>
	public bool AddValidation { get; set; } = true;

	internal OpenApiOptions(OpenApiDocument document)
	{
		Document = document;
	}

	internal void Validate()
	{
		if (InteractivePath is null) return;

		if (DocumentPath is null)
			throw new InvalidOperationException(
				$"`{nameof(InteractivePath)}` is set, but `{nameof(DocumentPath)}` is not. " +
				"The reference page reads the description over HTTP, so the description must be served.");

		if (!DocumentFormats.HasFlag(OpenApiFormats.Json))
			throw new InvalidOperationException(
				$"`{nameof(InteractivePath)}` is set, but `{nameof(DocumentFormats)}` does not include JSON. " +
				"The reference page reads the description as JSON.");
	}
}
