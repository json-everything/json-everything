using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

// TODO: .NET 5+ would have these methods marked with `RequiresUnreferencedCodeAttribute` to warn against tree trimming
internal static class JsonSerializerOptionsExtensions
{

	/// <summary>
	/// Read and convert the JSON to an arbitrary type.
	/// </summary>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
	/// <param name="arbitraryType">The <see cref="Type"/> to convert to.</param>
	/// <returns>The value that was converted.</returns>
	internal static object? Read(this JsonSerializerOptions options, ref Utf8JsonReader reader, Type arbitraryType)
	{
		var converter = ArbitraryDeserializerBase.GetConverter(arbitraryType);
		return converter.Read(ref reader, options);
	}


	private abstract class ArbitraryDeserializerBase
	{
		private static ConcurrentDictionary<Type, ArbitraryDeserializerBase> _deserializerCache = new ConcurrentDictionary<Type, ArbitraryDeserializerBase>();

		public static ArbitraryDeserializerBase GetConverter(Type arbitraryType)
		{
			return _deserializerCache.GetOrAdd(arbitraryType, t => (ArbitraryDeserializerBase)Activator.CreateInstance(typeof(ArbitraryDeserializer<>).MakeGenericType(t)));
		}

		public abstract object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options);
	}

	private class ArbitraryDeserializer<T> : ArbitraryDeserializerBase
	{
		public override object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var converter = (JsonConverter<T>)options.GetConverter(typeof(T));

			return converter.Read(ref reader, typeof(T), options);
		}
	}
}
