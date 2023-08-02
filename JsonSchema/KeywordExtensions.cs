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
	static KeywordExtensions()
	{
		_keywordEvaluationGroups = new Dictionary<Type, int>();

		var allTypes = GetAllKeywordTypes().ToList();

		var allDependencies = allTypes.ToDictionary(x => x, x => x.GetCustomAttributes<DependsOnAnnotationsFromAttribute>().Select(x => x.DependentType));

		_keywordEvaluationGroups[typeof(SchemaKeyword)] = -2;
		_keywordEvaluationGroups[typeof(IdKeyword)] = -1;
		_keywordEvaluationGroups[typeof(UnevaluatedItemsKeyword)] = int.MaxValue;
		_keywordEvaluationGroups[typeof(UnevaluatedPropertiesKeyword)] = int.MaxValue;

		allTypes.Remove(typeof(SchemaKeyword));
		allTypes.Remove(typeof(IdKeyword));
		allTypes.Remove(typeof(UnevaluatedItemsKeyword));
		allTypes.Remove(typeof(UnevaluatedPropertiesKeyword));

		allTypes.Remove(typeof(UnrecognizedKeyword));

		var groupId = 0;
		while (allTypes.Count != 0)
		{
			var groupKeywords = allTypes.Where(x => allDependencies[x].All(d => !allTypes.Contains(d))).ToArray();

			foreach (var groupKeyword in groupKeywords)
			{
				_keywordEvaluationGroups[groupKeyword] = groupId;
				allTypes.Remove(groupKeyword);
			}

			groupId++;
		}
	}

	private static IEnumerable<Type> GetAllKeywordTypes() =>
		typeof(IJsonSchemaKeyword).Assembly
			.GetTypes()
			.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
			            !t.IsAbstract &&
			            !t.IsInterface);

	private static readonly Dictionary<Type, string> _keywordNames =
		GetAllKeywordTypes()
			.Where(t => t != typeof(UnrecognizedKeyword))
			.ToDictionary(t => t, t => t.GetCustomAttribute<SchemaKeywordAttribute>().Name);

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

	private static readonly Dictionary<Type, int> _keywordEvaluationGroups;

	/// <summary>
	/// Gets the keyword priority.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The priority.</returns>
	public static long Priority(this IJsonSchemaKeyword keyword)
	{
		if (keyword == null) throw new ArgumentNullException(nameof(keyword));

		var keywordType = keyword.GetType();

		if (!_keywordEvaluationGroups.TryGetValue(keywordType, out var priority))
		{
			var keywordDependencies = keywordType.GetCustomAttributes<DependsOnAnnotationsFromAttribute>().Select(x => x.DependentType);
			var dependencyPriorities = _keywordEvaluationGroups.Join(keywordDependencies,
				eg => eg.Key,
				kd => kd,
				(eg, _) => eg)
				.ToArray();
			priority = dependencyPriorities.Length == 0
				? 0
				: dependencyPriorities.Max(x => x.Value);
		}

		return priority;
	}

	private static readonly Dictionary<Type, SpecVersion> _versionDeclarations =
		GetAllKeywordTypes()
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

	/// <summary>
	/// Gets the specification versions supported by a keyword.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The specification versions as a single flags value.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="keyword"/> is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the keyword has no <see cref="SchemaSpecVersionAttribute"/> declarations.</exception>
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

		return _keywordEvaluationGroups.Where(x => x.Value > 0).Any(x => x.Key == keywordType);
	}
}