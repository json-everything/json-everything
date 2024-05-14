using System;

namespace Json.Schema.Generation;

/// <summary>
/// Allows custom `$defs` key naming functionality.
/// </summary>
public interface ITypeNameGenerator
{
	/// <summary>
	/// Generates a `$defs` key for a type.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <returns>A string to use for the type; null to use the library-provided behavior.</returns>
	string? GenerateName(Type type);
}