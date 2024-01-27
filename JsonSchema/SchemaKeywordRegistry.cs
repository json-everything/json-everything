using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
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
	private static readonly ConcurrentDictionary<Type, JsonTypeInfo> _keywordTypeInfos;
	private static readonly ConcurrentDictionary<Type, IJsonSchemaKeyword> _nullKeywords;
	internal static bool RequiresDynamicSerialization { get; private set; }
#if !NET6_0_OR_GREATER
		= true;
#endif

	internal static IEnumerable<Type> KeywordTypes => _keywords.Values;

	static SchemaKeywordRegistry()
	{
		var keywordData = new (Type, string, JsonTypeInfo)[]
		{
			( typeof(AdditionalItemsKeyword), AdditionalItemsKeyword.Name, JsonSchemaSerializerContext.Default.AdditionalItemsKeyword ),
			( typeof(AdditionalPropertiesKeyword), AdditionalPropertiesKeyword.Name, JsonSchemaSerializerContext.Default.AdditionalPropertiesKeyword ),
			( typeof(AllOfKeyword), AllOfKeyword.Name, JsonSchemaSerializerContext.Default.AllOfKeyword ),
			( typeof(AnchorKeyword), AnchorKeyword.Name, JsonSchemaSerializerContext.Default.AnchorKeyword ),
			( typeof(AnyOfKeyword), AnyOfKeyword.Name, JsonSchemaSerializerContext.Default.AnyOfKeyword ),
			( typeof(CommentKeyword), CommentKeyword.Name, JsonSchemaSerializerContext.Default.CommentKeyword ),
			( typeof(ConstKeyword), ConstKeyword.Name, JsonSchemaSerializerContext.Default.ConstKeyword ),
			( typeof(ContainsKeyword), ContainsKeyword.Name, JsonSchemaSerializerContext.Default.ContainsKeyword ),
			( typeof(ContentEncodingKeyword), ContentEncodingKeyword.Name, JsonSchemaSerializerContext.Default.ContentEncodingKeyword ),
			( typeof(ContentMediaTypeKeyword), ContentMediaTypeKeyword.Name, JsonSchemaSerializerContext.Default.ContentMediaTypeKeyword ),
			( typeof(ContentSchemaKeyword), ContentSchemaKeyword.Name, JsonSchemaSerializerContext.Default.ContentSchemaKeyword ),
			( typeof(DefaultKeyword), DefaultKeyword.Name, JsonSchemaSerializerContext.Default.DefaultKeyword ),
			( typeof(DefinitionsKeyword), DefinitionsKeyword.Name, JsonSchemaSerializerContext.Default.DefinitionsKeyword ),
			( typeof(DefsKeyword), DefsKeyword.Name, JsonSchemaSerializerContext.Default.DefsKeyword ),
			( typeof(DependenciesKeyword), DependenciesKeyword.Name, JsonSchemaSerializerContext.Default.DependenciesKeyword ),
			( typeof(DependentRequiredKeyword), DependentRequiredKeyword.Name, JsonSchemaSerializerContext.Default.DependentRequiredKeyword ),
			( typeof(DependentSchemasKeyword), DependentSchemasKeyword.Name, JsonSchemaSerializerContext.Default.DependentSchemasKeyword ),
			( typeof(DeprecatedKeyword), DeprecatedKeyword.Name, JsonSchemaSerializerContext.Default.DeprecatedKeyword ),
			( typeof(DescriptionKeyword), DescriptionKeyword.Name, JsonSchemaSerializerContext.Default.DescriptionKeyword ),
			( typeof(DynamicAnchorKeyword), DynamicAnchorKeyword.Name, JsonSchemaSerializerContext.Default.DynamicAnchorKeyword ),
			( typeof(DynamicRefKeyword), DynamicRefKeyword.Name, JsonSchemaSerializerContext.Default.DynamicRefKeyword ),
			( typeof(ElseKeyword), ElseKeyword.Name, JsonSchemaSerializerContext.Default.ElseKeyword ),
			( typeof(EnumKeyword), EnumKeyword.Name, JsonSchemaSerializerContext.Default.EnumKeyword ),
			( typeof(ExamplesKeyword), ExamplesKeyword.Name, JsonSchemaSerializerContext.Default.ExamplesKeyword ),
			( typeof(ExclusiveMaximumKeyword), ExclusiveMaximumKeyword.Name, JsonSchemaSerializerContext.Default.ExclusiveMaximumKeyword ),
			( typeof(ExclusiveMinimumKeyword), ExclusiveMinimumKeyword.Name, JsonSchemaSerializerContext.Default.ExclusiveMinimumKeyword ),
			( typeof(FormatKeyword), FormatKeyword.Name, JsonSchemaSerializerContext.Default.FormatKeyword ),
			( typeof(IdKeyword), IdKeyword.Name, JsonSchemaSerializerContext.Default.IdKeyword ),
			( typeof(IfKeyword), IfKeyword.Name, JsonSchemaSerializerContext.Default.IfKeyword ),
			( typeof(ItemsKeyword), ItemsKeyword.Name, JsonSchemaSerializerContext.Default.ItemsKeyword ),
			( typeof(MaxContainsKeyword), MaxContainsKeyword.Name, JsonSchemaSerializerContext.Default.MaxContainsKeyword ),
			( typeof(MaximumKeyword), MaximumKeyword.Name, JsonSchemaSerializerContext.Default.MaximumKeyword ),
			( typeof(MaxItemsKeyword), MaxItemsKeyword.Name, JsonSchemaSerializerContext.Default.MaxItemsKeyword ),
			( typeof(MaxLengthKeyword), MaxLengthKeyword.Name, JsonSchemaSerializerContext.Default.MaxLengthKeyword ),
			( typeof(MaxPropertiesKeyword), MaxPropertiesKeyword.Name, JsonSchemaSerializerContext.Default.MaxPropertiesKeyword ),
			( typeof(MinContainsKeyword), MinContainsKeyword.Name, JsonSchemaSerializerContext.Default.MinContainsKeyword ),
			( typeof(MinimumKeyword), MinimumKeyword.Name, JsonSchemaSerializerContext.Default.MinimumKeyword ),
			( typeof(MinItemsKeyword), MinItemsKeyword.Name, JsonSchemaSerializerContext.Default.MinItemsKeyword ),
			( typeof(MinLengthKeyword), MinLengthKeyword.Name, JsonSchemaSerializerContext.Default.MinLengthKeyword ),
			( typeof(MinPropertiesKeyword), MinPropertiesKeyword.Name, JsonSchemaSerializerContext.Default.MinPropertiesKeyword ),
			( typeof(MultipleOfKeyword), MultipleOfKeyword.Name, JsonSchemaSerializerContext.Default.MultipleOfKeyword ),
			( typeof(NotKeyword), NotKeyword.Name, JsonSchemaSerializerContext.Default.NotKeyword ),
			( typeof(OneOfKeyword), OneOfKeyword.Name, JsonSchemaSerializerContext.Default.OneOfKeyword ),
			( typeof(PatternKeyword), PatternKeyword.Name, JsonSchemaSerializerContext.Default.PatternKeyword ),
			( typeof(PatternPropertiesKeyword), PatternPropertiesKeyword.Name, JsonSchemaSerializerContext.Default.PatternPropertiesKeyword ),
			( typeof(PrefixItemsKeyword), PrefixItemsKeyword.Name, JsonSchemaSerializerContext.Default.PrefixItemsKeyword ),
			( typeof(PropertiesKeyword), PropertiesKeyword.Name, JsonSchemaSerializerContext.Default.PropertiesKeyword ),
			( typeof(PropertyDependenciesKeyword), PropertyDependenciesKeyword.Name, JsonSchemaSerializerContext.Default.PropertyDependenciesKeyword ),
			( typeof(PropertyNamesKeyword), PropertyNamesKeyword.Name, JsonSchemaSerializerContext.Default.PropertyNamesKeyword ),
			( typeof(ReadOnlyKeyword), ReadOnlyKeyword.Name, JsonSchemaSerializerContext.Default.ReadOnlyKeyword ),
			( typeof(RecursiveAnchorKeyword), RecursiveAnchorKeyword.Name, JsonSchemaSerializerContext.Default.RecursiveAnchorKeyword ),
			( typeof(RecursiveRefKeyword), RecursiveRefKeyword.Name, JsonSchemaSerializerContext.Default.RecursiveRefKeyword ),
			( typeof(RefKeyword), RefKeyword.Name, JsonSchemaSerializerContext.Default.RefKeyword ),
			( typeof(RequiredKeyword), RequiredKeyword.Name, JsonSchemaSerializerContext.Default.RequiredKeyword ),
			( typeof(SchemaKeyword), SchemaKeyword.Name, JsonSchemaSerializerContext.Default.SchemaKeyword ),
			( typeof(ThenKeyword), ThenKeyword.Name, JsonSchemaSerializerContext.Default.ThenKeyword ),
			( typeof(TitleKeyword), TitleKeyword.Name, JsonSchemaSerializerContext.Default.TitleKeyword ),
			( typeof(TypeKeyword), TypeKeyword.Name, JsonSchemaSerializerContext.Default.TypeKeyword ),
			( typeof(UnevaluatedItemsKeyword), UnevaluatedItemsKeyword.Name, JsonSchemaSerializerContext.Default.UnevaluatedItemsKeyword ),
			( typeof(UnevaluatedPropertiesKeyword), UnevaluatedPropertiesKeyword.Name, JsonSchemaSerializerContext.Default.UnevaluatedPropertiesKeyword ),
			( typeof(UniqueItemsKeyword), UniqueItemsKeyword.Name, JsonSchemaSerializerContext.Default.UniqueItemsKeyword ),
			( typeof(VocabularyKeyword), VocabularyKeyword.Name, JsonSchemaSerializerContext.Default.VocabularyKeyword ),
			( typeof(WriteOnlyKeyword), WriteOnlyKeyword.Name, JsonSchemaSerializerContext.Default.WriteOnlyKeyword ),
		};

		_keywords = new ConcurrentDictionary<string, Type>(keywordData.ToDictionary(x => x.Item2, x => x.Item1));

		using var document = JsonDocument.Parse("null");
		_nullKeywords = new ConcurrentDictionary<Type, IJsonSchemaKeyword>
		{
			[typeof(ConstKeyword)] = new ConstKeyword(null),
			[typeof(DefaultKeyword)] = new DefaultKeyword(null)
		};
		_keywordTypeInfos = new ConcurrentDictionary<Type, JsonTypeInfo>(keywordData.ToDictionary(x => x.Item1, x => x.Item3));
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

		// Once someone registers a keyword, we track that we'll need to use reflection on this later.
		RequiresDynamicSerialization = true;
	}

	/// <summary>
	/// Registers a new keyword type.
	/// </summary>
	/// <typeparam name="T">The keyword type.</typeparam>
	/// <param name="typeInfo">JsonTypeInfo for the keyword type</param>
	public static void Register<T>(JsonTypeInfo typeInfo)
		where T : IJsonSchemaKeyword
	{
		var keyword = typeof(T).GetCustomAttribute<SchemaKeywordAttribute>() ??
					  throw new ArgumentException($"Keyword implementation `{typeof(T).Name}` does not carry `{nameof(SchemaKeywordAttribute)}`");

#if NET8_0_OR_GREATER // TypeInfo.Converter is part of System.Text.Json 8.x
		var converter = typeInfo.Converter as IJsonConverterReadWrite ??
			throw new ArgumentException("Keyword Converter must implement IJsonConverterReadWrite or Json.More.AotCompatibleJsonConverter to be AOT compatible");
#endif

		_keywords[keyword.Name] = typeof(T);
		_keywordTypeInfos[typeof(T)] = typeInfo;
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
		_keywordTypeInfos.TryRemove(typeof(T), out _);
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


	/// <summary>
	/// Get the JsonTypeInfo for a given keywordType
	/// </summary>
	/// <param name="keywordType">Type to look up</param>
	/// <param name="jsonTypeInfo">JsonTypeInfo for serialization</param>
	/// <remarks>
	/// This supports callers that use the JsonTypeInfo overloads of JsonSerializer methods.
	/// </remarks>
	internal static bool TryGetTypeInfo(Type keywordType, out JsonTypeInfo? jsonTypeInfo)
	{
		return _keywordTypeInfos.TryGetValue(keywordType, out jsonTypeInfo);
	}

	internal static IJsonSchemaKeyword? GetNullValuedKeyword(Type keywordType)
	{
		return _nullKeywords.TryGetValue(keywordType, out var instance) ? instance : null;
	}
}