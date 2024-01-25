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
			( typeof(DefinitionsKeyword), "definitions", JsonSchemaSerializationContext.Default.DefinitionsKeyword ),
			( typeof(MinPropertiesKeyword), "minProperties", JsonSchemaSerializationContext.Default.MinPropertiesKeyword ),
			( typeof(ContentSchemaKeyword), "contentSchema", JsonSchemaSerializationContext.Default.ContentSchemaKeyword ),
			( typeof(UnevaluatedPropertiesKeyword), "unevaluatedProperties", JsonSchemaSerializationContext.Default.UnevaluatedPropertiesKeyword ),
			( typeof(AllOfKeyword), "allOf", JsonSchemaSerializationContext.Default.AllOfKeyword ),
			( typeof(SchemaKeyword), "$schema", JsonSchemaSerializationContext.Default.SchemaKeyword ),
			( typeof(AdditionalPropertiesKeyword), "additionalProperties", JsonSchemaSerializationContext.Default.AdditionalPropertiesKeyword ),
			( typeof(MaxItemsKeyword), "maxItems", JsonSchemaSerializationContext.Default.MaxItemsKeyword ),
			( typeof(DynamicAnchorKeyword), "$dynamicAnchor", JsonSchemaSerializationContext.Default.DynamicAnchorKeyword ),
			( typeof(DependentSchemasKeyword), "dependentSchemas", JsonSchemaSerializationContext.Default.DependentSchemasKeyword ),
			( typeof(PrefixItemsKeyword), "prefixItems", JsonSchemaSerializationContext.Default.PrefixItemsKeyword ),
			( typeof(ConstKeyword), "const", JsonSchemaSerializationContext.Default.ConstKeyword ),
			( typeof(PatternPropertiesKeyword), "patternProperties", JsonSchemaSerializationContext.Default.PatternPropertiesKeyword ),
			( typeof(MinItemsKeyword), "minItems", JsonSchemaSerializationContext.Default.MinItemsKeyword ),
			( typeof(WriteOnlyKeyword), "writeOnly", JsonSchemaSerializationContext.Default.WriteOnlyKeyword ),
			( typeof(TitleKeyword), "title", JsonSchemaSerializationContext.Default.TitleKeyword ),
			( typeof(DependenciesKeyword), "dependencies", JsonSchemaSerializationContext.Default.DependenciesKeyword ),
			( typeof(ContentEncodingKeyword), "contentEncoding", JsonSchemaSerializationContext.Default.ContentEncodingKeyword ),
			( typeof(NotKeyword), "not", JsonSchemaSerializationContext.Default.NotKeyword ),
			( typeof(ElseKeyword), "else", JsonSchemaSerializationContext.Default.ElseKeyword ),
			( typeof(AnyOfKeyword), "anyOf", JsonSchemaSerializationContext.Default.AnyOfKeyword ),
			( typeof(MaximumKeyword), "maximum", JsonSchemaSerializationContext.Default.MaximumKeyword ),
			( typeof(MinimumKeyword), "minimum", JsonSchemaSerializationContext.Default.MinimumKeyword ),
			( typeof(AnchorKeyword), "$anchor", JsonSchemaSerializationContext.Default.AnchorKeyword ),
			( typeof(OneOfKeyword), "oneOf", JsonSchemaSerializationContext.Default.OneOfKeyword ),
			( typeof(MaxContainsKeyword), "maxContains", JsonSchemaSerializationContext.Default.MaxContainsKeyword ),
			( typeof(ContainsKeyword), "contains", JsonSchemaSerializationContext.Default.ContainsKeyword ),
			( typeof(ThenKeyword), "then", JsonSchemaSerializationContext.Default.ThenKeyword ),
			( typeof(DeprecatedKeyword), "deprecated", JsonSchemaSerializationContext.Default.DeprecatedKeyword ),
			( typeof(DefaultKeyword), "default", JsonSchemaSerializationContext.Default.DefaultKeyword ),
			( typeof(DefsKeyword), "$defs", JsonSchemaSerializationContext.Default.DefsKeyword ),
			( typeof(VocabularyKeyword), "$vocabulary", JsonSchemaSerializationContext.Default.VocabularyKeyword ),
			( typeof(DescriptionKeyword), "description", JsonSchemaSerializationContext.Default.DescriptionKeyword ),
			( typeof(PropertiesKeyword), "properties", JsonSchemaSerializationContext.Default.PropertiesKeyword ),
			( typeof(FormatKeyword), "format", JsonSchemaSerializationContext.Default.FormatKeyword ),
			( typeof(RefKeyword), "$ref", JsonSchemaSerializationContext.Default.RefKeyword ),
			( typeof(MaxLengthKeyword), "maxLength", JsonSchemaSerializationContext.Default.MaxLengthKeyword ),
			( typeof(RequiredKeyword), "required", JsonSchemaSerializationContext.Default.RequiredKeyword ),
			( typeof(ExamplesKeyword), "examples", JsonSchemaSerializationContext.Default.ExamplesKeyword ),
			( typeof(MinContainsKeyword), "minContains", JsonSchemaSerializationContext.Default.MinContainsKeyword ),
			( typeof(CommentKeyword), "$comment", JsonSchemaSerializationContext.Default.CommentKeyword ),
			( typeof(UnevaluatedItemsKeyword), "unevaluatedItems", JsonSchemaSerializationContext.Default.UnevaluatedItemsKeyword ),
			( typeof(IfKeyword), "if", JsonSchemaSerializationContext.Default.IfKeyword ),
			( typeof(TypeKeyword), "type", JsonSchemaSerializationContext.Default.TypeKeyword ),
			( typeof(MinLengthKeyword), "minLength", JsonSchemaSerializationContext.Default.MinLengthKeyword ),
			( typeof(RecursiveAnchorKeyword), "$recursiveAnchor", JsonSchemaSerializationContext.Default.RecursiveAnchorKeyword ),
			( typeof(PropertyDependenciesKeyword), "propertyDependencies", JsonSchemaSerializationContext.Default.PropertyDependenciesKeyword ),
			( typeof(UniqueItemsKeyword), "uniqueItems", JsonSchemaSerializationContext.Default.UniqueItemsKeyword ),
			( typeof(DependentRequiredKeyword), "dependentRequired", JsonSchemaSerializationContext.Default.DependentRequiredKeyword ),
			( typeof(IdKeyword), "$id", JsonSchemaSerializationContext.Default.IdKeyword ),
			( typeof(ReadOnlyKeyword), "readOnly", JsonSchemaSerializationContext.Default.ReadOnlyKeyword ),
			( typeof(PropertyNamesKeyword), "propertyNames", JsonSchemaSerializationContext.Default.PropertyNamesKeyword ),
			( typeof(MultipleOfKeyword), "multipleOf", JsonSchemaSerializationContext.Default.MultipleOfKeyword ),
			( typeof(EnumKeyword), "enum", JsonSchemaSerializationContext.Default.EnumKeyword ),
			( typeof(ExclusiveMinimumKeyword), "exclusiveMinimum", JsonSchemaSerializationContext.Default.ExclusiveMinimumKeyword ),
			( typeof(ContentMediaTypeKeyword), "contentMediaType", JsonSchemaSerializationContext.Default.ContentMediaTypeKeyword ),
			( typeof(PatternKeyword), "pattern", JsonSchemaSerializationContext.Default.PatternKeyword ),
			( typeof(ItemsKeyword), "items", JsonSchemaSerializationContext.Default.ItemsKeyword ),
			( typeof(DynamicRefKeyword), "$dynamicRef", JsonSchemaSerializationContext.Default.DynamicRefKeyword ),
			( typeof(RecursiveRefKeyword), "$recursiveRef", JsonSchemaSerializationContext.Default.RecursiveRefKeyword ),
			( typeof(MaxPropertiesKeyword), "maxProperties", JsonSchemaSerializationContext.Default.MaxPropertiesKeyword ),
			( typeof(AdditionalItemsKeyword), "additionalItems", JsonSchemaSerializationContext.Default.AdditionalItemsKeyword ),
			( typeof(ExclusiveMaximumKeyword), "exclusiveMaximum", JsonSchemaSerializationContext.Default.ExclusiveMaximumKeyword ),
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