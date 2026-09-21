using System.Collections.Generic;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Configures how the generated OpenAPI description is published.
/// </summary>
public class OpenApiOptions
{
	/// <summary>
	/// Gets or sets the base route the description is served from.  Defaults to `/openapi`.
	/// </summary>
	/// <remarks>
	/// The route resolves both formats by extension — `/openapi.json` and `/openapi.yaml` —
	/// and honors the `Accept` header when the extension is omitted.
	/// </remarks>
	public string Route { get; set; } = "/openapi";

	/// <summary>
	/// Gets or sets whether the description is served over HTTP.  Defaults to true.
	/// </summary>
	/// <remarks>
	/// When false the description is still built, still available from the service
	/// provider, and still written to <see cref="OutputPaths"/>; it is simply not exposed.
	/// </remarks>
	public bool Publish { get; set; } = true;

	/// <summary>
	/// Gets or sets paths the description is written to at startup.  The format of each
	/// file follows its extension.
	/// </summary>
	public IList<string> OutputPaths { get; set; } = [];

	/// <summary>
	/// Gets or sets the OpenAPI version.  Defaults to `3.1.1`.
	/// </summary>
	public string OpenApiVersion { get; set; } = "3.1.1";

	/// <summary>
	/// Gets or sets the API title.  Defaults to the entry assembly's name.
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// Gets or sets the API version.  Defaults to `1.0.0`.
	/// </summary>
	public string Version { get; set; } = "1.0.0";
}
