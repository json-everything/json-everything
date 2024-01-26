using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Json.More;

namespace Json.Schema;

// TODO: .NET 5+ would have these methods marked with `RequiresUnreferencedCodeAttribute` to warn against tree trimming
internal static class JsonSerializerOptionsExtensions
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
		public override object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options, JsonTypeInfo? typeInfo)
		{
			typeInfo ??= options.GetTypeInfo(typeof(T));
			var converter = (JsonConverter<T>)typeInfo.Converter;

			return converter.Read(ref reader, typeof(T), options);
		}
	}

	internal static object? Read(this JsonSerializerOptions options, ref Utf8JsonReader reader, Type arbitraryType, JsonTypeInfo? typeInfo = null)
	{
		typeInfo ??= options.GetTypeInfo(arbitraryType);
		var converter = typeInfo.Converter;

#if NET8_0_OR_GREATER // Needs default interface method implementations
		// Try using the AOT-friendly interface first.
		if (converter is IJsonConverterReadWrite converterReadWrite)
		{
			return converterReadWrite.Read(ref reader, arbitraryType, options);
		}
#endif

		// The converter is just a JsonConverter<T> so we need to go through reflection to get it.
		// AOT-aware callers should not have gotten this far.
#pragma warning disable IL2026, IL3050
		var deserializer = ArbitraryDeserializer.GetConverter(arbitraryType);
#pragma warning restore IL2026, IL3050
		return deserializer.Read(ref reader, options);
	}
}
