using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
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
	private static readonly ConcurrentDictionary<Type, JsonSerializerContext> _keywordTypeInfoResolvers = new();
	private static readonly ConcurrentDictionary<Type, IJsonSchemaKeyword> _nullKeywords;

#if NET8_0_OR_GREATER
	internal static IJsonTypeInfoResolver[] ExtraTypeInfoResolvers => _keywordTypeInfoResolvers.Values.Distinct().ToArray();
#endif

	internal static IEnumerable<Type> KeywordTypes => _keywords.Values;

	static SchemaKeywordRegistry()
	{
		var keywordData = new (Type, string)[]
		{
			( typeof(AdditionalItemsKeyword), AdditionalItemsKeyword.Name),
			( typeof(AdditionalPropertiesKeyword), AdditionalPropertiesKeyword.Name),
			( typeof(AllOfKeyword), AllOfKeyword.Name),
			( typeof(AnchorKeyword), AnchorKeyword.Name),
			( typeof(AnyOfKeyword), AnyOfKeyword.Name),
			( typeof(CommentKeyword), CommentKeyword.Name),
			( typeof(ConstKeyword), ConstKeyword.Name),
			( typeof(ContainsKeyword), ContainsKeyword.Name),
			( typeof(ContentEncodingKeyword), ContentEncodingKeyword.Name),
			( typeof(ContentMediaTypeKeyword), ContentMediaTypeKeyword.Name),
			( typeof(ContentSchemaKeyword), ContentSchemaKeyword.Name),
			( typeof(DefaultKeyword), DefaultKeyword.Name),
			( typeof(DefinitionsKeyword), DefinitionsKeyword.Name),
			( typeof(DefsKeyword), DefsKeyword.Name),
			( typeof(DependenciesKeyword), DependenciesKeyword.Name),
			( typeof(DependentRequiredKeyword), DependentRequiredKeyword.Name),
			( typeof(DependentSchemasKeyword), DependentSchemasKeyword.Name),
			( typeof(DeprecatedKeyword), DeprecatedKeyword.Name),
			( typeof(DescriptionKeyword), DescriptionKeyword.Name),
			( typeof(DynamicAnchorKeyword), DynamicAnchorKeyword.Name),
			( typeof(DynamicRefKeyword), DynamicRefKeyword.Name),
			( typeof(ElseKeyword), ElseKeyword.Name),
			( typeof(EnumKeyword), EnumKeyword.Name),
			( typeof(ExamplesKeyword), ExamplesKeyword.Name),
			( typeof(ExclusiveMaximumKeyword), ExclusiveMaximumKeyword.Name),
			( typeof(ExclusiveMinimumKeyword), ExclusiveMinimumKeyword.Name),
			( typeof(FormatKeyword), FormatKeyword.Name),
			( typeof(IdKeyword), IdKeyword.Name),
			( typeof(IfKeyword), IfKeyword.Name),
			( typeof(ItemsKeyword), ItemsKeyword.Name),
			( typeof(MaxContainsKeyword), MaxContainsKeyword.Name),
			( typeof(MaximumKeyword), MaximumKeyword.Name),
			( typeof(MaxItemsKeyword), MaxItemsKeyword.Name),
			( typeof(MaxLengthKeyword), MaxLengthKeyword.Name),
			( typeof(MaxPropertiesKeyword), MaxPropertiesKeyword.Name),
			( typeof(MinContainsKeyword), MinContainsKeyword.Name),
			( typeof(MinimumKeyword), MinimumKeyword.Name),
			( typeof(MinItemsKeyword), MinItemsKeyword.Name),
			( typeof(MinLengthKeyword), MinLengthKeyword.Name),
			( typeof(MinPropertiesKeyword), MinPropertiesKeyword.Name),
			( typeof(MultipleOfKeyword), MultipleOfKeyword.Name),
			( typeof(NotKeyword), NotKeyword.Name),
			( typeof(OneOfKeyword), OneOfKeyword.Name),
			( typeof(PatternKeyword), PatternKeyword.Name),
			( typeof(PatternPropertiesKeyword), PatternPropertiesKeyword.Name),
			( typeof(PrefixItemsKeyword), PrefixItemsKeyword.Name),
			( typeof(PropertiesKeyword), PropertiesKeyword.Name),
			( typeof(PropertyDependenciesKeyword), PropertyDependenciesKeyword.Name),
			( typeof(PropertyNamesKeyword), PropertyNamesKeyword.Name),
			( typeof(ReadOnlyKeyword), ReadOnlyKeyword.Name),
			( typeof(RecursiveAnchorKeyword), RecursiveAnchorKeyword.Name),
			( typeof(RecursiveRefKeyword), RecursiveRefKeyword.Name),
			( typeof(RefKeyword), RefKeyword.Name),
			( typeof(RequiredKeyword), RequiredKeyword.Name),
			( typeof(SchemaKeyword), SchemaKeyword.Name),
			( typeof(ThenKeyword), ThenKeyword.Name),
			( typeof(TitleKeyword), TitleKeyword.Name),
			( typeof(TypeKeyword), TypeKeyword.Name),
			( typeof(UnevaluatedItemsKeyword), UnevaluatedItemsKeyword.Name),
			( typeof(UnevaluatedPropertiesKeyword), UnevaluatedPropertiesKeyword.Name),
			( typeof(UniqueItemsKeyword), UniqueItemsKeyword.Name),
			( typeof(VocabularyKeyword), VocabularyKeyword.Name),
			( typeof(WriteOnlyKeyword), WriteOnlyKeyword.Name),
		};

		_keywords = new ConcurrentDictionary<string, Type>(keywordData.ToDictionary(x => x.Item2, x => x.Item1));

		using var document = JsonDocument.Parse("null");
		_nullKeywords = new ConcurrentDictionary<Type, IJsonSchemaKeyword>
		{
			[typeof(ConstKeyword)] = new ConstKeyword(null),
			[typeof(DefaultKeyword)] = new DefaultKeyword(null)
		};
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

#if NET8_0_OR_GREATER // TypeInfo.Converter is part of System.Text.Json 8.x
		var typeInfo = typeContext.GetTypeInfo(typeof(T)) ??
					   throw new ArgumentException($"Keyword implementation `{typeof(T).Name}` does not have a JsonTypeInfo");
		var converter = typeInfo.Converter as IJsonConverterReadWrite ??
			throw new ArgumentException("Keyword Converter must implement IJsonConverterReadWrite or Json.More.AotCompatibleJsonConverter to be AOT compatible");
#endif

		_keywords[keyword.Name] = typeof(T);
		_keywordTypeInfoResolvers[typeof(T)] = typeContext;

		JsonSchema.InvalidateTypeInfoResolver();
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
		_keywordTypeInfoResolvers.TryRemove(typeof(T), out _);
	}

	/// <summary>
	/// Gets the implementation for a given keyword name.
	/// </summary>
	/// <param name="keyword">The keyword name.</param>
	/// <returns>The keyword type, if registered; otherwise null.</returns>
	public static Type? GetImplementationType(string keyword)
	{
		return _keywords.TryGetValue(keyword, out var implementationType)
			? implementationType
			: null;
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
		return _nullKeywords.TryGetValue(keywordType, out var instance) ? instance : null;
	}
}