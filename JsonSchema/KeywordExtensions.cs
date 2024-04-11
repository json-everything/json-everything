using System;

namespace Json.Schema;

/// <summary>
/// Some extensions for <see cref="IJsonSchemaKeyword"/>.
/// </summary>
public static class KeywordExtensions
{
	/// <summary>
	/// Gets the keyword string.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The keyword string.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="keyword"/> is null.</exception>
	/// <exception cref="InvalidOperationException">The keyword does not carry the <see cref="SchemaKeywordAttribute"/>.</exception>
	[Obsolete("This method has been replaced by SchemaKeywordRegistry.Keyword(IJsonSchemaKeyword).")]
	public static string Keyword(IJsonSchemaKeyword keyword) => keyword.Keyword();

	/// <summary>
	/// Gets the keyword string.
	/// </summary>
	/// <param name="keywordType">The keyword type.</param>
	/// <returns>The keyword string.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="keywordType"/> is null.</exception>
	/// <exception cref="InvalidOperationException">The keyword does not carry the <see cref="SchemaKeywordAttribute"/>.</exception>
	[Obsolete("This method has been replaced by SchemaKeywordRegistry.Keyword(Type).")]
	public static string Keyword(Type keywordType) => keywordType.Keyword();

	/// <summary>
	/// Gets the keyword priority.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The priority.</returns>
	[Obsolete("This method has been replaced by SchemaKeywordRegistry.GetPriority().")]
	public static long Priority(this IJsonSchemaKeyword keyword) => keyword.GetType().GetPriority();

	/// <summary>
	/// Determines if a keyword is declared by a given version of the JSON Schema specification.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <param name="version">The queried version.</param>
	/// <returns>true if the keyword is supported by the version; false otherwise</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="keyword"/> is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the keyword has no <see cref="SchemaSpecVersionAttribute"/> declarations.</exception>
	[Obsolete("This method has been replaced by SchemaKeywordRegistry.SupportsVersion().")]
	public static bool SupportsVersion(IJsonSchemaKeyword keyword, SpecVersion version) => keyword.SupportsVersion(version);

	/// <summary>
	/// Gets the specification versions supported by a keyword.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The specification versions as a single flags value.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="keyword"/> is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the keyword has no <see cref="SchemaSpecVersionAttribute"/> declarations.</exception>
	[Obsolete("This method is no longer used and will be removed with the next major version.")]
	public static SpecVersion VersionsSupported(this IJsonSchemaKeyword keyword) => SpecVersion.Unspecified;
}