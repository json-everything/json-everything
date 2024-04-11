using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Manages which keywords are known by the system.
/// </summary>
/// <remarks>
/// Because the deserialization process relies on keywords being registered,
/// this class cannot be an instance class like the other registries in this
/// library.  Therefore keywords are registered for all schemas.
/// </remarks>
public static class SchemaKeywordRegistry
{
	private static readonly ConcurrentDictionary<string, Type> _keywords;
	private static readonly ConcurrentDictionary<Type, string> _keywordNames;
	private static readonly ConcurrentDictionary<Type, IJsonSchemaKeyword> _nullKeywords;
	// This maps external types to their TypeInfoResolvers. Built-in keywords don't need this as we already have them
	// in our default JsonSerializerContext.
	private static readonly ConcurrentDictionary<Type, JsonSerializerContext> _keywordTypeInfoResolvers;
	private static readonly ConcurrentDictionary<Type, int> _keywordEvaluationGroups;
	private static readonly ConcurrentDictionary<Type, SpecVersion> _versionDeclarations;

	internal static IEnumerable<Type> KeywordTypes => _keywords.Values;

	static SchemaKeywordRegistry()
	{
		_keywordNames = new ConcurrentDictionary<Type, string>
		{
			[ typeof(AdditionalItemsKeyword)] = AdditionalItemsKeyword.Name,
			[ typeof(AdditionalPropertiesKeyword)] = AdditionalPropertiesKeyword.Name,
			[ typeof(AllOfKeyword)] = AllOfKeyword.Name,
			[ typeof(AnchorKeyword)] = AnchorKeyword.Name,
			[ typeof(AnyOfKeyword)] = AnyOfKeyword.Name,
			[ typeof(CommentKeyword)] = CommentKeyword.Name,
			[ typeof(ConstKeyword)] = ConstKeyword.Name,
			[ typeof(ContainsKeyword)] = ContainsKeyword.Name,
			[ typeof(ContentEncodingKeyword)] = ContentEncodingKeyword.Name,
			[ typeof(ContentMediaTypeKeyword)] = ContentMediaTypeKeyword.Name,
			[ typeof(ContentSchemaKeyword)] = ContentSchemaKeyword.Name,
			[ typeof(DefaultKeyword)] = DefaultKeyword.Name,
			[ typeof(DefinitionsKeyword)] = DefinitionsKeyword.Name,
			[ typeof(DefsKeyword)] = DefsKeyword.Name,
			[ typeof(DependenciesKeyword)] = DependenciesKeyword.Name,
			[ typeof(DependentRequiredKeyword)] = DependentRequiredKeyword.Name,
			[ typeof(DependentSchemasKeyword)] = DependentSchemasKeyword.Name,
			[ typeof(DeprecatedKeyword)] = DeprecatedKeyword.Name,
			[ typeof(DescriptionKeyword)] = DescriptionKeyword.Name,
			[ typeof(DynamicAnchorKeyword)] = DynamicAnchorKeyword.Name,
			[ typeof(DynamicRefKeyword)] = DynamicRefKeyword.Name,
			[ typeof(ElseKeyword)] = ElseKeyword.Name,
			[ typeof(EnumKeyword)] = EnumKeyword.Name,
			[ typeof(ExamplesKeyword)] = ExamplesKeyword.Name,
			[ typeof(ExclusiveMaximumKeyword)] = ExclusiveMaximumKeyword.Name,
			[ typeof(ExclusiveMinimumKeyword)] = ExclusiveMinimumKeyword.Name,
			[ typeof(FormatKeyword)] = FormatKeyword.Name,
			[ typeof(IdKeyword)] = IdKeyword.Name,
			[ typeof(IfKeyword)] = IfKeyword.Name,
			[ typeof(ItemsKeyword)] = ItemsKeyword.Name,
			[ typeof(MaxContainsKeyword)] = MaxContainsKeyword.Name,
			[ typeof(MaximumKeyword)] = MaximumKeyword.Name,
			[ typeof(MaxItemsKeyword)] = MaxItemsKeyword.Name,
			[ typeof(MaxLengthKeyword)] = MaxLengthKeyword.Name,
			[ typeof(MaxPropertiesKeyword)] = MaxPropertiesKeyword.Name,
			[ typeof(MinContainsKeyword)] = MinContainsKeyword.Name,
			[ typeof(MinimumKeyword)] = MinimumKeyword.Name,
			[ typeof(MinItemsKeyword)] = MinItemsKeyword.Name,
			[ typeof(MinLengthKeyword)] = MinLengthKeyword.Name,
			[ typeof(MinPropertiesKeyword)] = MinPropertiesKeyword.Name,
			[ typeof(MultipleOfKeyword)] = MultipleOfKeyword.Name,
			[ typeof(NotKeyword)] = NotKeyword.Name,
			[ typeof(OneOfKeyword)] = OneOfKeyword.Name,
			[ typeof(PatternKeyword)] = PatternKeyword.Name,
			[ typeof(PatternPropertiesKeyword)] = PatternPropertiesKeyword.Name,
			[ typeof(PrefixItemsKeyword)] = PrefixItemsKeyword.Name,
			[ typeof(PropertiesKeyword)] = PropertiesKeyword.Name,
			[ typeof(PropertyDependenciesKeyword)] = PropertyDependenciesKeyword.Name,
			[ typeof(PropertyNamesKeyword)] = PropertyNamesKeyword.Name,
			[ typeof(ReadOnlyKeyword)] = ReadOnlyKeyword.Name,
			[ typeof(RecursiveAnchorKeyword)] = RecursiveAnchorKeyword.Name,
			[ typeof(RecursiveRefKeyword)] = RecursiveRefKeyword.Name,
			[ typeof(RefKeyword)] = RefKeyword.Name,
			[ typeof(RequiredKeyword)] = RequiredKeyword.Name,
			[ typeof(SchemaKeyword)] = SchemaKeyword.Name,
			[ typeof(ThenKeyword)] = ThenKeyword.Name,
			[ typeof(TitleKeyword)] = TitleKeyword.Name,
			[ typeof(TypeKeyword)] = TypeKeyword.Name,
			[ typeof(UnevaluatedItemsKeyword)] = UnevaluatedItemsKeyword.Name,
			[ typeof(UnevaluatedPropertiesKeyword)] = UnevaluatedPropertiesKeyword.Name,
			[ typeof(UniqueItemsKeyword)] = UniqueItemsKeyword.Name,
			[ typeof(VocabularyKeyword)] = VocabularyKeyword.Name,
			[ typeof(WriteOnlyKeyword)] = WriteOnlyKeyword.Name
		};

		_keywords = new ConcurrentDictionary<string, Type>(_keywordNames.ToDictionary(x => x.Value, x => x.Key));
		_keywordTypeInfoResolvers = new ConcurrentDictionary<Type, JsonSerializerContext>(_keywordNames.ToDictionary(x => x.Key, _ => (JsonSerializerContext)JsonSchemaSerializerContext.Default));
		_versionDeclarations = new ConcurrentDictionary<Type, SpecVersion>(_keywordNames
			.ToDictionary(t => t.Key, t => t.Key.GetCustomAttributes<SchemaSpecVersionAttribute>()
				.Aggregate(SpecVersion.Unspecified, (c, x) => c | x.Version)));

		_nullKeywords = new ConcurrentDictionary<Type, IJsonSchemaKeyword>
		{
			[typeof(ConstKeyword)] = new ConstKeyword(null),
			[typeof(DefaultKeyword)] = new DefaultKeyword(null)
		};

		_keywordEvaluationGroups = [];
		RecalculatePriorities();

		// HACK - need to touch this to initialize the type and register everything
		_ = Vocabularies.Core201909;
	}

	/// <summary>
	/// Registers a new keyword type.
	/// </summary>
	/// <typeparam name="T">The keyword type.</typeparam>
	[RequiresDynamicCode("For AOT support, use Register<T> that takes a JsonTypeInfo. Using this method requires reflection later.")]
	public static void Register<T>()
		where T : IJsonSchemaKeyword
	{
		var keyword = typeof(T).GetCustomAttribute<SchemaKeywordAttribute>() ??
		              throw new ArgumentException($"Keyword implementation `{typeof(T).Name}` does not carry `{nameof(SchemaKeywordAttribute)}`");

		_keywords[keyword.Name] = typeof(T);
		_keywordNames[typeof(T)] = keyword.Name;
		_versionDeclarations[typeof(T)] = typeof(T).GetCustomAttributes<SchemaSpecVersionAttribute>()
			.Aggregate(SpecVersion.Unspecified, (c, x) => c | x.Version);
		RecalculatePriorities();
	}

	/// <summary>
	/// Registers a new keyword type.
	/// </summary>
	/// <typeparam name="T">The keyword type.</typeparam>
	/// <param name="typeContext">JsonTypeInfo for the keyword type</param>
	public static void Register<T>(JsonSerializerContext typeContext)
		where T : IJsonSchemaKeyword
	{
		var keyword = typeof(T).GetCustomAttribute<SchemaKeywordAttribute>() ??
					  throw new ArgumentException($"Keyword implementation `{typeof(T).Name}` does not carry `{nameof(SchemaKeywordAttribute)}`");

		var typeInfo = typeContext.GetTypeInfo(typeof(T)) ??
					   throw new ArgumentException($"Keyword implementation `{typeof(T).Name}` does not have a JsonTypeInfo");
		_ = typeInfo.Converter as IWeaklyTypedJsonConverter ??
			throw new ArgumentException("Keyword Converter must implement IJsonConverterReadWrite or AotCompatibleJsonConverter to be AOT compatible");

		_keywords[keyword.Name] = typeof(T);
		_keywordNames[typeof(T)] = keyword.Name;
		_keywordTypeInfoResolvers[typeof(T)] = typeContext;
		_versionDeclarations[typeof(T)] = typeof(T).GetCustomAttributes<SchemaSpecVersionAttribute>()
			.Aggregate(SpecVersion.Unspecified, (c, x) => c | x.Version);
		RecalculatePriorities();
	}

	/// <summary>
	/// Unregisters a keyword type.
	/// </summary>
	/// <typeparam name="T">The keyword type.</typeparam>
	public static void Unregister<T>()
		where T : IJsonSchemaKeyword
	{
		var keyword = typeof(T).GetCustomAttribute<SchemaKeywordAttribute>() ??
		              throw new ArgumentException($"Keyword implementation `{typeof(T).Name}` does not carry `{nameof(SchemaKeywordAttribute)}`");

		_keywords.TryRemove(keyword.Name, out _);
		_keywordNames.TryRemove(typeof(T), out _);
		_keywordTypeInfoResolvers.TryRemove(typeof(T), out _);
		_versionDeclarations.TryRemove(typeof(T), out _);

		RecalculatePriorities();
	}

	/// <summary>
	/// Gets the implementation for a given keyword name.
	/// </summary>
	/// <param name="keyword">The keyword name.</param>
	/// <returns>The keyword type, if registered; otherwise null.</returns>
	public static Type? GetImplementationType(string keyword)
	{
		return _keywords.GetValueOrDefault(keyword);
	}

	internal static JsonTypeInfo? GetTypeInfo(Type keywordType)
	{
		if (_keywordTypeInfoResolvers.TryGetValue(keywordType, out var context)) return context.GetTypeInfo(keywordType)!;

		// A keyword was registered without a JsonTypeInfo; use reflection
		if (KeywordTypes.Contains(keywordType)) return null;

		// The keyword is unknown
		return JsonSchemaSerializerContext.Default.GetTypeInfo(typeof(UnrecognizedKeyword))!;
	}

	/// <summary>
	/// Registers a null-value for a keyword.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="nullKeyword"></param>
	/// <remarks>
	/// This is important for keywords that accept null, like `default` and `const`.  Without
	/// this step, the serializer will skip keywords that have nulls.
	/// </remarks>
	public static void RegisterNullValue<T>(T nullKeyword)
		where T : IJsonSchemaKeyword
	{
		_nullKeywords[typeof(T)] = nullKeyword;
	}

	internal static IJsonSchemaKeyword? GetNullValuedKeyword(Type keywordType)
	{
		return _nullKeywords.GetValueOrDefault(keywordType);
	}

	/// <summary>
	/// Gets the keyword priority.
	/// </summary>
	/// <param name="keywordType">The keyword type.</param>
	/// <returns>The priority.</returns>
	public static int GetPriority(this Type keywordType)
	{
		return _keywordEvaluationGroups.TryGetValue(keywordType, out var priority)
			? priority
			: keywordType == typeof(UnrecognizedKeyword)
				? 0
				: throw new ArgumentException($"Keyword of type '{keywordType}' not registered");
	}

	private static void RecalculatePriorities()
	{
		_keywordEvaluationGroups.Clear();

		_keywordEvaluationGroups[typeof(SchemaKeyword)] = -2;
		_keywordEvaluationGroups[typeof(IdKeyword)] = -1;
		_keywordEvaluationGroups[typeof(UnevaluatedItemsKeyword)] = int.MaxValue;
		_keywordEvaluationGroups[typeof(UnevaluatedPropertiesKeyword)] = int.MaxValue;

		var allTypes = _keywords.Values.ToList();
		var allDependencies = allTypes.ToDictionary(x => x, x => x.GetCustomAttributes<DependsOnAnnotationsFromAttribute>().Select(y => y.DependentType));

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

	internal static bool ProducesDependentAnnotations(this Type keywordType)
	{
		if (keywordType == null) throw new ArgumentNullException(nameof(keywordType));

		return keywordType.GetPriority() > 0;
	}

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
		return _keywordNames.TryGetValue(keywordType, out var name)
			? name
			: throw new InvalidOperationException($"Type {keywordType.Name} must be decorated with {nameof(SchemaKeywordAttribute)}");
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

		return _keywordNames.TryGetValue(keywordType, out var name)
			? name
			: throw new InvalidOperationException($"Type {keywordType.Name} must be decorated with {nameof(SchemaKeywordAttribute)}");
	}
}