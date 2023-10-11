using System;
using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace Json.Schema.Data;

/// <summary>
/// Provides access to external data.
/// </summary>
public class ExternalDataRegistry
{
	/// <summary>
	/// Gets or sets a method to download external references.
	/// </summary>
	/// <remarks>
	/// The default method simply attempts to download the resource.  There is no
	/// caching involved.
	/// </remarks>
	public static Func<Uri, JsonNode?>? Fetch { get; set; }

	/// <summary>
	/// Provides a registry for known external data sources.
	/// </summary>
	/// <remarks>
	/// This property stores full JSON documents retrievable by URI.  If the desired
	/// value exists as a sub-value of a document, a JSON Pointer URI fragment identifier
	/// should be used in the `data` keyword do identify the exact value location.
	///
	/// This registry will be checked before attempting to fetch the data.
	/// </remarks>
	public static ConcurrentDictionary<Uri, JsonNode> Registry { get; } = new();
}