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
	/// <summary>
	/// Gets the configuration options used for the build process.
	/// </summary>
	public BuildOptions Options { get; }
	/// <summary>
	/// Gets or sets the base URI used to resolve relative resource references.
	/// </summary>
	public Uri BaseUri { get; set; }
	/// <summary>
	/// Gets or sets the local value from which a new schema is to be built.
	/// </summary>
	public JsonElement LocalSchema { get; set; }
	/// <summary>
	/// Gets or set the JSON Schema dialect to use in the build process.
	/// </summary>
	public Dialect Dialect { get; set; }
	/// <summary>
	/// Gets or sets the path to the <see cref="LocalSchema"/> in addition to the keyword.
	/// </summary>
	/// <remarks>For example, for the `properties` keyword, this pointer indicates the property name.</remarks>
	public JsonPointer RelativePath { get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	[Obsolete("This is only for advanced usage.")]
	public JsonPointer PathFromResourceRoot { get; set; } = JsonPointer.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

	internal BuildContext(BuildOptions options, Uri baseUri)
	{
		Options = options;
		BaseUri = baseUri;
		Dialect = Options.Dialect;
	}

	/// <summary>
	/// Creates a copy of a build context from the one that was used to build a keyword.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <returns>A copy of the build context.</returns>
	public static BuildContext From(KeywordData keyword) => keyword.Context;
}