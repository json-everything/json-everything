using System;
using System.Collections.Generic;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// The formats a description can be served in.
/// </summary>
[Flags]
public enum OpenApiFormats
{
	/// <summary>
	/// No format; the description is not served.
	/// </summary>
	None = 0,
	/// <summary>
	/// Served as JSON, at the document path with a `.json` extension.
	/// </summary>
	Json = 1,
	/// <summary>
	/// Served as YAML, at the document path with a `.yaml` or `.yml` extension.
	/// </summary>
	Yaml = 2
}

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
	/// Gets or sets the path the description is served from, without an extension.  The
	/// extensions come from <see cref="DocumentFormats"/>.  Defaults to `/openapi`.
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
	/// Gets the paths the description is written to at startup.  The format of each file
	/// follows its extension.
	/// </summary>
	public IList<string> OutputPaths { get; } = [];

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
