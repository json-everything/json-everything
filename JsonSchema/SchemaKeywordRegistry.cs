using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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
			( typeof(AdditionalItemsKeyword), AdditionalItemsKeyword.Name, JsonSchemaSerializationContext.Default.AdditionalItemsKeyword ),
			( typeof(AdditionalPropertiesKeyword), AdditionalPropertiesKeyword.Name, JsonSchemaSerializationContext.Default.AdditionalPropertiesKeyword ),
			( typeof(AllOfKeyword), AllOfKeyword.Name, JsonSchemaSerializationContext.Default.AllOfKeyword ),
			( typeof(AnchorKeyword), AnchorKeyword.Name, JsonSchemaSerializationContext.Default.AnchorKeyword ),
			( typeof(AnyOfKeyword), AnyOfKeyword.Name, JsonSchemaSerializationContext.Default.AnyOfKeyword ),
			( typeof(CommentKeyword), CommentKeyword.Name, JsonSchemaSerializationContext.Default.CommentKeyword ),
			( typeof(ConstKeyword), ConstKeyword.Name, JsonSchemaSerializationContext.Default.ConstKeyword ),
			( typeof(ContainsKeyword), ContainsKeyword.Name, JsonSchemaSerializationContext.Default.ContainsKeyword ),
			( typeof(ContentEncodingKeyword), ContentEncodingKeyword.Name, JsonSchemaSerializationContext.Default.ContentEncodingKeyword ),
			( typeof(ContentMediaTypeKeyword), ContentMediaTypeKeyword.Name, JsonSchemaSerializationContext.Default.ContentMediaTypeKeyword ),
			( typeof(ContentSchemaKeyword), ContentSchemaKeyword.Name, JsonSchemaSerializationContext.Default.ContentSchemaKeyword ),
			( typeof(DefaultKeyword), DefaultKeyword.Name, JsonSchemaSerializationContext.Default.DefaultKeyword ),
			( typeof(DefinitionsKeyword), DefinitionsKeyword.Name, JsonSchemaSerializationContext.Default.DefinitionsKeyword ),
			( typeof(DefsKeyword), DefsKeyword.Name, JsonSchemaSerializationContext.Default.DefsKeyword ),
			( typeof(DependenciesKeyword), DependenciesKeyword.Name, JsonSchemaSerializationContext.Default.DependenciesKeyword ),
			( typeof(DependentRequiredKeyword), DependentRequiredKeyword.Name, JsonSchemaSerializationContext.Default.DependentRequiredKeyword ),
			( typeof(DependentSchemasKeyword), DependentSchemasKeyword.Name, JsonSchemaSerializationContext.Default.DependentSchemasKeyword ),
			( typeof(DeprecatedKeyword), DeprecatedKeyword.Name, JsonSchemaSerializationContext.Default.DeprecatedKeyword ),
			( typeof(DescriptionKeyword), DescriptionKeyword.Name, JsonSchemaSerializationContext.Default.DescriptionKeyword ),
			( typeof(DynamicAnchorKeyword), DynamicAnchorKeyword.Name, JsonSchemaSerializationContext.Default.DynamicAnchorKeyword ),
			( typeof(DynamicRefKeyword), DynamicRefKeyword.Name, JsonSchemaSerializationContext.Default.DynamicRefKeyword ),
			( typeof(ElseKeyword), ElseKeyword.Name, JsonSchemaSerializationContext.Default.ElseKeyword ),
			( typeof(EnumKeyword), EnumKeyword.Name, JsonSchemaSerializationContext.Default.EnumKeyword ),
			( typeof(ExamplesKeyword), ExamplesKeyword.Name, JsonSchemaSerializationContext.Default.ExamplesKeyword ),
			( typeof(ExclusiveMaximumKeyword), ExclusiveMaximumKeyword.Name, JsonSchemaSerializationContext.Default.ExclusiveMaximumKeyword ),
			( typeof(ExclusiveMinimumKeyword), ExclusiveMinimumKeyword.Name, JsonSchemaSerializationContext.Default.ExclusiveMinimumKeyword ),
			( typeof(FormatKeyword), FormatKeyword.Name, JsonSchemaSerializationContext.Default.FormatKeyword ),
			( typeof(IdKeyword), IdKeyword.Name, JsonSchemaSerializationContext.Default.IdKeyword ),
			( typeof(IfKeyword), IfKeyword.Name, JsonSchemaSerializationContext.Default.IfKeyword ),
			( typeof(ItemsKeyword), ItemsKeyword.Name, JsonSchemaSerializationContext.Default.ItemsKeyword ),
			( typeof(MaxContainsKeyword), MaxContainsKeyword.Name, JsonSchemaSerializationContext.Default.MaxContainsKeyword ),
			( typeof(MaximumKeyword), MaximumKeyword.Name, JsonSchemaSerializationContext.Default.MaximumKeyword ),
			( typeof(MaxItemsKeyword), MaxItemsKeyword.Name, JsonSchemaSerializationContext.Default.MaxItemsKeyword ),
			( typeof(MaxLengthKeyword), MaxLengthKeyword.Name, JsonSchemaSerializationContext.Default.MaxLengthKeyword ),
			( typeof(MaxPropertiesKeyword), MaxPropertiesKeyword.Name, JsonSchemaSerializationContext.Default.MaxPropertiesKeyword ),
			( typeof(MinContainsKeyword), MinContainsKeyword.Name, JsonSchemaSerializationContext.Default.MinContainsKeyword ),
			( typeof(MinimumKeyword), MinimumKeyword.Name, JsonSchemaSerializationContext.Default.MinimumKeyword ),
			( typeof(MinItemsKeyword), MinItemsKeyword.Name, JsonSchemaSerializationContext.Default.MinItemsKeyword ),
			( typeof(MinLengthKeyword), MinLengthKeyword.Name, JsonSchemaSerializationContext.Default.MinLengthKeyword ),
			( typeof(MinPropertiesKeyword), MinPropertiesKeyword.Name, JsonSchemaSerializationContext.Default.MinPropertiesKeyword ),
			( typeof(MultipleOfKeyword), MultipleOfKeyword.Name, JsonSchemaSerializationContext.Default.MultipleOfKeyword ),
			( typeof(NotKeyword), NotKeyword.Name, JsonSchemaSerializationContext.Default.NotKeyword ),
			( typeof(OneOfKeyword), OneOfKeyword.Name, JsonSchemaSerializationContext.Default.OneOfKeyword ),
			( typeof(PatternKeyword), PatternKeyword.Name, JsonSchemaSerializationContext.Default.PatternKeyword ),
			( typeof(PatternPropertiesKeyword), PatternPropertiesKeyword.Name, JsonSchemaSerializationContext.Default.PatternPropertiesKeyword ),
			( typeof(PrefixItemsKeyword), PrefixItemsKeyword.Name, JsonSchemaSerializationContext.Default.PrefixItemsKeyword ),
			( typeof(PropertiesKeyword), PropertiesKeyword.Name, JsonSchemaSerializationContext.Default.PropertiesKeyword ),
			( typeof(PropertyDependenciesKeyword), PropertyDependenciesKeyword.Name, JsonSchemaSerializationContext.Default.PropertyDependenciesKeyword ),
			( typeof(PropertyNamesKeyword), PropertyNamesKeyword.Name, JsonSchemaSerializationContext.Default.PropertyNamesKeyword ),
			( typeof(ReadOnlyKeyword), ReadOnlyKeyword.Name, JsonSchemaSerializationContext.Default.ReadOnlyKeyword ),
			( typeof(RecursiveAnchorKeyword), RecursiveAnchorKeyword.Name, JsonSchemaSerializationContext.Default.RecursiveAnchorKeyword ),
			( typeof(RecursiveRefKeyword), RecursiveRefKeyword.Name, JsonSchemaSerializationContext.Default.RecursiveRefKeyword ),
			( typeof(RefKeyword), RefKeyword.Name, JsonSchemaSerializationContext.Default.RefKeyword ),
			( typeof(RequiredKeyword), RequiredKeyword.Name, JsonSchemaSerializationContext.Default.RequiredKeyword ),
			( typeof(SchemaKeyword), SchemaKeyword.Name, JsonSchemaSerializationContext.Default.SchemaKeyword ),
			( typeof(ThenKeyword), ThenKeyword.Name, JsonSchemaSerializationContext.Default.ThenKeyword ),
			( typeof(TitleKeyword), TitleKeyword.Name, JsonSchemaSerializationContext.Default.TitleKeyword ),
			( typeof(TypeKeyword), TypeKeyword.Name, JsonSchemaSerializationContext.Default.TypeKeyword ),
			( typeof(UnevaluatedItemsKeyword), UnevaluatedItemsKeyword.Name, JsonSchemaSerializationContext.Default.UnevaluatedItemsKeyword ),
			( typeof(UnevaluatedPropertiesKeyword), UnevaluatedPropertiesKeyword.Name, JsonSchemaSerializationContext.Default.UnevaluatedPropertiesKeyword ),
			( typeof(UniqueItemsKeyword), UniqueItemsKeyword.Name, JsonSchemaSerializationContext.Default.UniqueItemsKeyword ),
			( typeof(VocabularyKeyword), VocabularyKeyword.Name, JsonSchemaSerializationContext.Default.VocabularyKeyword ),
			( typeof(WriteOnlyKeyword), WriteOnlyKeyword.Name, JsonSchemaSerializationContext.Default.WriteOnlyKeyword ),
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
	/// <param name="name">name of the keyword</param>
	/// <param name="typeInfo">JsonTypeInfo for the keyword type</param>
	public static void Register<T>(string name, JsonTypeInfo typeInfo)
		where T : IJsonSchemaKeyword
	{
		_keywords[name] = typeof(T);
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
	public static bool TryGetTypeInfo(Type keywordType, out JsonTypeInfo? jsonTypeInfo)
	{
		return _keywordTypeInfos.TryGetValue(keywordType, out jsonTypeInfo);
	}

	internal static IJsonSchemaKeyword? GetNullValuedKeyword(Type keywordType)
	{
		return _nullKeywords.TryGetValue(keywordType, out var instance) ? instance : null;
	}
}