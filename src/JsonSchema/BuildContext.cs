using System;
using System.Text.Json;
using Json.Pointer;

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
	public Uri BaseUri { get; set; }
	public JsonElement LocalSchema { get; set; }
	public Dialect Dialect { get; set; }
	public JsonPointer RelativePath { get; set; }

	internal JsonPointer PathFromResourceRoot { get; set; } = JsonPointer.Empty;

	internal BuildContext(BuildOptions options, Uri baseUri)
	{
		Options = options;
		BaseUri = baseUri;
		Dialect = Options.Dialect;
	}
}