using System;

namespace Json.Schema;

/// <summary>
/// Indicates which JSON Schema specification versions are supported by a keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class SchemaSpecVersionAttribute : Attribute
{
	/// <summary>
	/// The supported version.
	/// </summary>
	public SpecVersion Version { get; }

	/// <summary>
	/// Creates a new <see cref="SchemaSpecVersionAttribute"/>.
	/// </summary>
	/// <param name="version">The supported version.</param>
	public SchemaSpecVersionAttribute(SpecVersion version)
	{
		Version = version;
	}
}