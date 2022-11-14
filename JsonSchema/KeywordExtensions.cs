using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Schema;

/// <summary>
/// Some extensions for <see cref="IJsonSchemaKeyword"/>.
/// </summary>
public static class KeywordExtensions
{
	private static readonly Dictionary<Type, string> _keywordNames =
		typeof(IJsonSchemaKeyword).Assembly
			.GetTypes()
			.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
						!t.IsAbstract &&
						!t.IsInterface &&
						t != typeof(UnrecognizedKeyword))
			.ToDictionary(t => t, t => t.GetCustomAttribute<SchemaKeywordAttribute>().Name);
	private static readonly Type[] _keywordDependencies =
		typeof(IJsonSchemaKeyword).Assembly
			.GetTypes()
			.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
						!t.IsAbstract &&
						!t.IsInterface &&
						t != typeof(UnrecognizedKeyword))
			.SelectMany(t => t.GetCustomAttributes<DependsOnAnnotationsFromAttribute>().Select(x => x.DependentType))
			.Distinct()
			.ToArray();

	/// <summary>
	/// Gets the keyword string.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The keyword string.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="keyword"/> is null.</exception>
	/// <exception cref="InvalidOperationException">The keyword does not carry the <see cref="SchemaKeywordAttribute"/>.</exception>
	public static string Keyword(this IJsonSchemaKeyword keyword)
	{
		if (keyword == null) throw new ArgumentNullException(nameof(keyword));

		if (keyword is UnrecognizedKeyword unrecognized) return unrecognized.Name;

		var keywordType = keyword.GetType();
		if (!_keywordNames.TryGetValue(keywordType, out var name))
		{
			name = keywordType.GetCustomAttribute<SchemaKeywordAttribute>()?.Name;
			if (name == null)
				throw new InvalidOperationException($"Type {keywordType.Name} must be decorated with {nameof(SchemaKeywordAttribute)}");

			_keywordNames[keywordType] = name;
		}

		return name;
	}

	/// <summary>
	/// Gets the keyword string.
	/// </summary>
	/// <param name="keywordType">The keyword type.</param>
	/// <returns>The keyword string.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="keywordType"/> is null.</exception>
	/// <exception cref="InvalidOperationException">The keyword does not carry the <see cref="SchemaKeywordAttribute"/>.</exception>
	public static string Keyword(this Type keywordType)
	{
		if (keywordType == null) throw new ArgumentNullException(nameof(keywordType));

		if (!_keywordNames.TryGetValue(keywordType, out var name))
		{
			name = keywordType.GetCustomAttribute<SchemaKeywordAttribute>()?.Name;
			if (name == null)
				throw new InvalidOperationException($"Type {keywordType.Name} must be decorated with {nameof(SchemaKeywordAttribute)}");

			_keywordNames[keywordType] = name;
		}

		return name;
	}

	private static readonly Dictionary<Type, long> _priorities =
		typeof(IJsonSchemaKeyword).Assembly
			.GetTypes()
			.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
						!t.IsAbstract &&
						!t.IsInterface)
			.ToDictionary(t => t, t => t.GetCustomAttribute<SchemaPriorityAttribute>()?.ActualPriority ?? 0);

	/// <summary>
	/// Gets the keyword priority.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The priority.</returns>
	public static long Priority(this IJsonSchemaKeyword keyword)
	{
		if (keyword == null) throw new ArgumentNullException(nameof(keyword));

		var keywordType = keyword.GetType();
		if (!_priorities.TryGetValue(keywordType, out var priority))
		{
			var priorityAttribute = keywordType.GetCustomAttribute<SchemaPriorityAttribute>();
			priority = priorityAttribute?.ActualPriority ?? 0;
			_priorities[keywordType] = priority;
		}

		return priority;
	}

	private static readonly Dictionary<Type, SpecVersion> _versionDeclarations =
		typeof(IJsonSchemaKeyword).Assembly
			.GetTypes()
			.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
						!t.IsAbstract &&
						!t.IsInterface)
			.ToDictionary(t => t, t => t.GetCustomAttributes<SchemaSpecVersionAttribute>()
				.Aggregate(SpecVersion.Unspecified, (c, x) => c | x.Version));

	/// <summary>
	/// Determines if a keyword is declared by a given version of the JSON Schema specification.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <param name="version">The queried version.</param>
	/// <returns>true if the keyword is supported by the version; false otherwise</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="keyword"/> is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the keyword has no <see cref="SchemaSpecVersionAttribute"/> declarations.</exception>
	public static bool SupportsVersion(this IJsonSchemaKeyword keyword, SpecVersion version)
	{
		if (keyword == null) throw new ArgumentNullException(nameof(keyword));

		var keywordType = keyword.GetType();
		if (!_versionDeclarations.TryGetValue(keywordType, out var supportedVersions))
		{
			supportedVersions = keywordType.GetCustomAttributes<SchemaSpecVersionAttribute>()
				.Aggregate(SpecVersion.Unspecified, (c, x) => c | x.Version);
			if (supportedVersions == SpecVersion.Unspecified)
				throw new InvalidOperationException($"Type {keywordType.Name} must be decorated with {nameof(SchemaSpecVersionAttribute)}");

			_versionDeclarations[keywordType] = supportedVersions;
		}

		return supportedVersions.HasFlag(version);
	}

	public static SpecVersion VersionsSupported(this IJsonSchemaKeyword keyword)
	{
		if (keyword == null) throw new ArgumentNullException(nameof(keyword));

		var keywordType = keyword.GetType();
		if (!_versionDeclarations.TryGetValue(keywordType, out var supportedVersions))
		{
			supportedVersions = keywordType.GetCustomAttributes<SchemaSpecVersionAttribute>()
				.Aggregate(SpecVersion.Unspecified, (c, x) => c | x.Version);
			if (supportedVersions == SpecVersion.Unspecified)
				throw new InvalidOperationException($"Type {keywordType.Name} must be decorated with {nameof(SchemaSpecVersionAttribute)}");

			_versionDeclarations[keywordType] = supportedVersions;
		}

		return supportedVersions;
	}

	internal static bool ProducesDependentAnnotations(this Type keywordType)
	{
		if (keywordType == null) throw new ArgumentNullException(nameof(keywordType));

		return _keywordDependencies.Contains(keywordType);
	}
}