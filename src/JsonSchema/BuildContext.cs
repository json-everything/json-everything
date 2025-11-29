using System;
using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Provides contextual information required during the construction of a schema, including build options, schema
/// documents, and dialect settings.
/// </summary>
/// <remarks>Use this structure to pass relevant build parameters processing keywords. The context encapsulates
/// both global and local schema data, as well as the base URI and dialect, ensuring consistent schema resolution
/// and validation throughout the build process.</remarks>
public struct BuildContext
{
	public BuildOptions Options { get; }
	public JsonElement RootSchema { get; }
	public Uri BaseUri { get; set; }
	public JsonElement LocalSchema { get; set; }
	public Dialect Dialect { get; set; }

	internal BuildContext(BuildOptions options, JsonElement rootSchema, Uri baseUri)
	{
		Options = options;
		RootSchema = rootSchema;
		BaseUri = baseUri;
		Dialect = Options.Dialect;
	}
}