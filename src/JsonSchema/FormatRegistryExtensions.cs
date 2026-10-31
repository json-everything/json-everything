using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

/// <summary>
/// Provides extensions for <see cref="FormatRegistry"/> to create new registries by adding
/// and removing individual formats
/// </summary>
public static class FormatRegistryExtensions
{
	private class FormatKeyComparer : IEqualityComparer<Format>
	{
		public static FormatKeyComparer Instance { get; } = new();

		private FormatKeyComparer()
		{
		}

		public bool Equals(Format? x, Format? y) => x?.Key == y?.Key;

		public int GetHashCode(Format? obj) => obj?.Key.GetHashCode() ?? 0;
	}

	/// <summary>
	/// Creates a new registry with the specified formats added.
	/// </summary>
	/// <param name="registry">The source registry.</param>
	/// <param name="formats">The formats to add.</param>
	/// <returns>A new registry.</returns>
	public static FormatRegistry With(this FormatRegistry registry, params IEnumerable<Format> formats) =>
		new(formats.Union(registry.GetFormats(), FormatKeyComparer.Instance));

	/// <summary>
	/// Creates a new registry with the specified formats removed.
	/// </summary>
	/// <param name="registry">The source registry.</param>
	/// <param name="formatKeys">The keys of the formats to remove.</param>
	/// <returns>A new registry.</returns>
	public static FormatRegistry Without(this FormatRegistry registry, params IEnumerable<string> formatKeys) =>
		new(registry.GetFormats().Where(x => !formatKeys.Contains(x.Key)));

	/// <summary>
	/// Creates a new registry with the specified formats removed.
	/// </summary>
	/// <param name="registry">The source registry.</param>
	/// <param name="formats">The formats to remove.</param>
	/// <returns>A new registry.</returns>
	public static FormatRegistry Without(this FormatRegistry registry, params IEnumerable<Format> formats) =>
		new(registry.GetFormats().Except(formats, FormatKeyComparer.Instance));
}