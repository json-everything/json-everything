using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

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

		[RequiresDynamicCode("Calls MakeGenericType")]
		[RequiresUnreferencedCode("Calls MakeGenericType")]
		public abstract object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options);
	}

	private class ArbitraryDeserializer<T> : ArbitraryDeserializer
	{
		[RequiresDynamicCode("Calls MakeGenericType")]
		[RequiresUnreferencedCode("Calls MakeGenericType")]
		public override object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var converter = (JsonConverter<T>)options.GetConverter(typeof(T));

			return converter.Read(ref reader, typeof(T), options);
		}
	}

	[RequiresDynamicCode("Calls MakeGenericType")]
	[RequiresUnreferencedCode("Calls MakeGenericType")]
	internal static object? Read(this JsonSerializerOptions options, ref Utf8JsonReader reader, Type arbitraryType)
	{
//#if NET6_0_OR_GREATER
//		if (options.TryGetTypeInfo(arbitraryType, out var typeinfo))
//		{
//			return JsonSerializer.Deserialize(ref reader, typeinfo);
//		}

//		// TODO: make the above TypeInfo path support the SchemaRegistry things.
//#endif

		var converter = ArbitraryDeserializer.GetConverter(arbitraryType);
		return converter.Read(ref reader, options);
	}
}
