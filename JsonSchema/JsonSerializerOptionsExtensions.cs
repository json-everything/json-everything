using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Extension methods for <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
	private abstract class ArbitraryDeserializer
	{
		private static readonly ConcurrentDictionary<Type, ArbitraryDeserializer> _deserializerCache = new();

		[RequiresDynamicCode("Calls MakeGenericType")]
		[RequiresUnreferencedCode("Calls MakeGenericType")]
		public static ArbitraryDeserializer GetConverter(Type arbitraryType)
		{
			return _deserializerCache.GetOrAdd(arbitraryType, t => (ArbitraryDeserializer)Activator.CreateInstance(typeof(ArbitraryDeserializer<>).MakeGenericType(t))!);
		}

		public abstract object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options, JsonTypeInfo? typeInfo = null);
	}

	private class ArbitraryDeserializer<T> : ArbitraryDeserializer
	{
		public override object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options, JsonTypeInfo? typeInfo = null)
		{
			typeInfo ??= options.GetTypeInfo(typeof(T));
			var converter = (JsonConverter<T>)typeInfo.Converter;

			return converter.Read(ref reader, typeof(T), options);
		}
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We won't use dynamic code if the JsonSerializerOptions come from the source generator.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We won't use dynamic code if the JsonSerializerOptions come from the source generator.")]
	internal static object? Read(this JsonSerializerOptions options, ref Utf8JsonReader reader, Type arbitraryType, JsonTypeInfo typeInfo)
	{
		var converter = typeInfo.Converter;

		// Try using the AOT-friendly interface first.
		if (converter is IWeaklyTypedJsonConverter converterReadWrite)
			return converterReadWrite.Read(ref reader, arbitraryType, options);

		// The converter is just a JsonConverter<T> so we need to go through reflection to get it.
		// AOT-aware callers should not have gotten this far.
		var deserializer = ArbitraryDeserializer.GetConverter(arbitraryType);
		return deserializer.Read(ref reader, options);
	}

	/// <summary>
	/// Adds serializer context information to the type resolver chain.
	/// </summary>
	/// <param name="options">The options.</param>
	/// <returns>The same options.</returns>
	/// <remarks>
	/// Also adds the context for <see cref="JsonPointer"/>.
	/// </remarks>
	public static JsonSerializerOptions WithJsonSchema(this JsonSerializerOptions options)
	{
		options.TypeInfoResolverChain.Add(JsonSchemaSerializerContext.Default);
		return options;
	}
}
