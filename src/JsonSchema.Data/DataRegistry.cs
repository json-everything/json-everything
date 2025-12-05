using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Json.Schema.Data;

/// <summary>
/// Provides a registry for associating URIs with JSON elements, supporting registration, retrieval, and optional
/// automatic fetching of data by URI.
/// </summary>
/// <remarks>The DataRegistry class enables applications to register and retrieve JSON data by URI, with support
/// for a global registry and customizable fetch logic. Registered data can be retrieved from both instance and global
/// registries. Thread safety is not guaranteed; callers should ensure appropriate synchronization if accessing
/// instances from multiple threads.</remarks>
public class DataRegistry
{
	private static readonly Uri _empty = new("https://json-everything.lib/");

	private readonly Dictionary<Uri, JsonElement> _registered = [];

	/// <summary>
	/// The global registry.
	/// </summary>
	public static DataRegistry Global { get; } = new();

	/// <summary>
	/// Gets or sets a method to enable automatic download of data by URI ID.
	/// </summary>
	public Func<Uri, JsonElement?> Fetch
	{
		get => field ??= _ => null;
		set;
	}

	/// <summary>
	/// Registers the specified JSON element with the given URI, replacing any existing element associated with that URI.
	/// </summary>
	/// <remarks>If an element is already registered for the given URI, it will be overwritten.</remarks>
	/// <param name="uri">The URI to associate with the JSON element. Must be an absolute or relative URI; relative URIs are resolved to
	/// absolute form.</param>
	/// <param name="element">The JSON element to register with the specified URI.</param>
	public void Register(Uri uri, JsonElement element)
	{
		uri = MakeAbsolute(uri);
		_registered[uri] = element;
	}

	/// <summary>
	/// Gets data by URI ID.
	/// </summary>
	/// <param name="uri">The URI ID.</param>
	/// <returns>
	/// The <see cref="JsonElement"/>, if registered in either this or the global registry;
	/// otherwise null.
	/// </returns>
	// For URI equality see https://docs.microsoft.com/en-us/dotnet/api/system.uri.op_equality?view=netcore-3.1
	// tl;dr - URI equality doesn't consider fragments
	public JsonElement? Get(Uri uri)
	{
		var resolved = _registered.GetValueOrDefault(uri);

		if (resolved.ValueKind == JsonValueKind.Undefined)
			resolved = Global._registered.GetValueOrDefault(uri);

		if (resolved.ValueKind == JsonValueKind.Undefined)
			return null;

		return resolved;
	}

	private static Uri MakeAbsolute(Uri? uri)
	{
		if (uri == null) return _empty;

		if (uri.IsAbsoluteUri) return uri;

		return _empty.Resolve(uri);
	}

	/// <summary>
	/// Provides a simple data fetch method that supports `http`, `https`, and `file` URI schemes.
	/// </summary>
	/// <param name="uri">The URI to fetch.</param>
	/// <returns>A JSON string representing the data</returns>
	/// <exception cref="FormatException">
	/// Thrown when the URI scheme is not `http`, `https`, or `file`.
	/// </exception>
	public static JsonNode? SimpleDownload(Uri uri)
	{
		switch (uri.Scheme)
		{
			case "http":
			case "https":
				return new HttpClient().GetStringAsync(uri).Result;
			case "file":
				var filename = Uri.UnescapeDataString(uri.AbsolutePath);
				return File.ReadAllText(filename);
			default:
				throw new FormatException($"URI scheme '{uri.Scheme}' is not supported.  Only HTTP(S) and local file system URIs are allowed.");
		}
	}
}
