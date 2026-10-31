using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

/// <summary>
/// Represents a thread-safe registry for managing and retrieving format definitions within the application.
/// </summary>
/// <remarks>Use the FormatRegistry to register, unregister, and look up format definitions by key. The static
/// Global property provides access to a shared, application-wide registry pre-populated with common formats. This class
/// is designed for scenarios where centralized management of formats is required, and is safe for use across multiple
/// threads.</remarks>
public class FormatRegistry
{
	private readonly ConcurrentDictionary<string, Format> _formats;

	/// <summary>
	/// Gets the global registry of formats available throughout the application.
	/// </summary>
	/// <remarks>Use this property to access a shared, application-wide instance of the format registry. The
	/// global registry is intended for scenarios where a centralized format store is required. This property is
	/// thread-safe.</remarks>
	public static FormatRegistry Global => V1;

	/// <summary>
	/// Gets the formats defined by the Draft 6 dialect.
	/// </summary>
	public static FormatRegistry Draft06 { get; } = new(
		Keywords.Draft06.Formats.DateTime,
		Formats.Email,
		Formats.Hostname,
		Formats.Ipv4,
		Formats.Ipv6,
		Formats.RelativeJsonPointer,
		Formats.Uri,
		Formats.UriReference,
		Formats.UriTemplate,
		Formats.JsonPointer
	);

	/// <summary>
	/// Gets the formats defined by the Draft 7 dialect.
	/// </summary>
	public static FormatRegistry Draft07 { get; } = new(
		Keywords.Draft06.Formats.DateTime,
		Formats.Date,
		Keywords.Draft07.Formats.Time,
		Formats.Email,
		Formats.IdnEmail,
		Formats.Hostname,
		Formats.IdnHostname,
		Formats.Ipv4,
		Formats.Ipv6,
		Formats.Uri,
		Formats.UriReference,
		Formats.Iri,
		Formats.IriReference,
		Formats.UriTemplate,
		Formats.JsonPointer,
		Formats.RelativeJsonPointer,
		Formats.Regex
	);

	/// <summary>
	/// Gets the formats defined by the Draft 2019-09 dialect.
	/// </summary>
	public static FormatRegistry Draft201909 { get; } = new(
		Keywords.Draft06.Formats.DateTime,
		Formats.Date,
		Keywords.Draft07.Formats.Time,
		Formats.Duration,
		Formats.Email,
		Formats.IdnEmail,
		Formats.Hostname,
		Formats.IdnHostname,
		Formats.Ipv4,
		Formats.Ipv6,
		Formats.Uri,
		Formats.UriReference,
		Formats.Iri,
		Formats.IriReference,
		Formats.Uuid,
		Formats.UriTemplate,
		Formats.JsonPointer,
		Formats.RelativeJsonPointer,
		Formats.Regex
	);

	/// <summary>
	/// Gets the formats defined by the Draft 2020-12 dialect.
	/// </summary>
	public static FormatRegistry Draft202012 { get; } = new(
		Keywords.Draft06.Formats.DateTime,
		Formats.Date,
		Keywords.Draft07.Formats.Time,
		Formats.Duration,
		Formats.Email,
		Formats.IdnEmail,
		Formats.Hostname,
		Formats.IdnHostname,
		Formats.Ipv4,
		Formats.Ipv6,
		Formats.Uri,
		Formats.UriReference,
		Formats.Iri,
		Formats.IriReference,
		Formats.Uuid,
		Formats.UriTemplate,
		Formats.JsonPointer,
		Formats.RelativeJsonPointer,
		Formats.Regex
	);

	/// <summary>
	/// Gets the formats defined by the V1 dialect.
	/// </summary>
	public static FormatRegistry V1 { get; } = new(
		Formats.DateTime,
		Formats.Date,
		Formats.Time,
		Formats.Duration,
		Formats.Email,
		Formats.IdnEmail,
		Formats.Hostname,
		Formats.IdnHostname,
		Formats.Ipv4,
		Formats.Ipv6,
		Formats.Uri,
		Formats.UriReference,
		Formats.Iri,
		Formats.IriReference,
		Formats.Uuid,
		Formats.UriTemplate,
		Formats.JsonPointer,
		Formats.RelativeJsonPointer,
		Formats.Regex
	);

	/// <summary>
	/// Creates a new format registry.
	/// </summary>
	/// <param name="formats">The list of formats for the registry to provide.</param>
	public FormatRegistry(params IEnumerable<Format> formats)
	{
		_formats = new ConcurrentDictionary<string, Format>(formats.ToDictionary(x => x.Key, x => x));
	}

	/// <summary>
	/// Registers the specified format for later retrieval by its key.
	/// </summary>
	/// <param name="format">The format to register. The format's key must be unique within the collection.</param>
	public void Register(Format format)
	{
		_formats[format.Key] = format;
	}

	/// <summary>
	/// Unregisters the format associated with the specified name, removing it from the collection if it exists.
	/// </summary>
	/// <remarks>If no format is registered with the specified name, this method has no effect. This method is
	/// thread-safe.</remarks>
	/// <param name="name">The name of the format to unregister. Cannot be null.</param>
	public void Unregister(string name)
	{
		_formats.TryRemove(name, out _);
	}

	/// <summary>
	/// Retrieves the format associated with the specified key, searching local and global collections.
	/// </summary>
	/// <remarks>If the format is not found in the local collection, the method searches the global collection
	/// before returning an unknown format. The returned format is never null.</remarks>
	/// <param name="key">The key that identifies the format to retrieve. Cannot be null.</param>
	/// <returns>The format associated with the specified key if found; otherwise, a format representing an unknown key.</returns>
	public Format Get(string key)
	{
		return _formats.GetValueOrDefault(key) ??
			Global._formats.GetValueOrDefault(key) ??
			Formats.CreateUnknown(key);
	}

	internal IEnumerable<Format> GetFormats() => _formats.Values.Distinct();
}