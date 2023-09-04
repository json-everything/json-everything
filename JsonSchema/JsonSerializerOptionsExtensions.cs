using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema;

// TODO: .NET 5+ would have these methods marked with `RequiresUnreferencedCodeAttribute` to warn against tree trimming
internal static class JsonSerializerOptionsExtensions
{
	public static JsonConverter<T> GetConverter<T>(this JsonSerializerOptions options)
	{
		return (JsonConverter<T>)options.GetConverter(typeof(T));
	}

	public static T? Read<T>(this JsonSerializerOptions options, ref Utf8JsonReader reader)
	{
		return options.GetConverter<T>().Read(ref reader, typeof(T), options);
	}

	public static object? Read(this JsonSerializerOptions options, ref Utf8JsonReader reader, Type arbitraryType)
	{
		var converter = (IArbitraryDeserializer)Activator.CreateInstance(typeof(ArbitraryDeserializer<>).MakeGenericType(arbitraryType));
		return converter.Read(ref reader, options);
	}


	private interface IArbitraryDeserializer
	{
		object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options);
	}

	private class ArbitraryDeserializer<T> : IArbitraryDeserializer
	{
		public object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var converter = (JsonConverter<T>)options.GetConverter(typeof(T));

			return converter.Read(ref reader, typeof(T), options);
		}
	}
}
