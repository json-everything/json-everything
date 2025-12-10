using System.Collections.Concurrent;
using System.Collections.Generic;

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
	private readonly ConcurrentDictionary<string, Format> _formats = new();

	/// <summary>
	/// Gets the global registry of formats available throughout the application.
	/// </summary>
	/// <remarks>Use this property to access a shared, application-wide instance of the format registry. The
	/// global registry is intended for scenarios where a centralized format store is required. This property is
	/// thread-safe.</remarks>
	public static FormatRegistry Global { get; } = new();

	static FormatRegistry()
	{
		Global.Register(Formats.Date);
		Global.Register(Formats.DateTime);
		Global.Register(Formats.Duration);
		Global.Register(Formats.Email);
		Global.Register(Formats.Hostname);
		Global.Register(Formats.IdnEmail);
		Global.Register(Formats.IdnHostname);
		Global.Register(Formats.Ipv4);
		Global.Register(Formats.Ipv6);
		Global.Register(Formats.Iri);
		Global.Register(Formats.IriReference);
		Global.Register(Formats.JsonPointer);
		Global.Register(Formats.Regex);
		Global.Register(Formats.RelativeJsonPointer);
		Global.Register(Formats.Time);
		Global.Register(Formats.Uri);
		Global.Register(Formats.UriReference);
		Global.Register(Formats.UriTemplate);
		Global.Register(Formats.Uuid);
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
}