using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Json.More;

/// <summary>
/// Provides extension functionality for <see cref="JsonSerializerOptions"/>.
/// </summary>
/// <remarks>
/// Most (if not all) of these extension methods are workarounds for
/// https://github.com/dotnet/runtime/issues/50205.
/// </remarks>
public static class JsonSerializerOptionsExtensions
{
	/// <summary>
	/// Returns the converter for the specified type.
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="typeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	/// <returns>An implementation of <see cref="JsonConverter{T}"/> as determined by the provided options</returns>
	public static JsonConverter<T> GetConverter<T>(this JsonSerializerOptions options, JsonTypeInfo? typeInfo)
	{
		var optionsConverter = (JsonConverter<T>?)options.Converters.FirstOrDefault(x => x is JsonConverter<T>);

		return optionsConverter ?? (JsonConverter<T>)(typeInfo ?? options.GetTypeInfo(typeof(T))).Converter;
	}

	/// <summary>
	/// Read and convert the JSON to T.
	/// </summary>
	/// <remarks>
	/// A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON is invalid.
	/// </remarks>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
	/// <param name="typeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	/// <returns>The value that was converted.</returns>
	public static T? Read<T>(this JsonSerializerOptions options, ref Utf8JsonReader reader, JsonTypeInfo<T>? typeInfo)
	{
		return options.GetConverter<T>(typeInfo).Read(ref reader, typeof(T), options);
	}

	/// <summary>
	/// Write a T to JSON.
	/// </summary>
	/// <remarks>
	/// A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON is invalid.
	/// </remarks>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="writer">The <see cref="Utf8JsonReader"/> to read from.</param>
	/// <param name="value">The value to serialize.</param>
	/// <param name="typeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	/// <returns>The value that was converted.</returns>
	public static void Write<T>(this JsonSerializerOptions options, Utf8JsonWriter writer, T value, JsonTypeInfo<T> typeInfo)
	{
		options.GetConverter<T>(typeInfo).Write(writer, value, options);
		//((JsonConverter<T>)typeInfo.Converter).Write(writer, value, options);
	}

	/// <summary>
	/// Read and convert the JSON to multiple T.
	/// </summary>
	/// <remarks>
	/// A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON is invalid.
	/// </remarks>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
	/// <param name="typeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	/// <returns>The value that was converted.</returns>
	public static List<T>? ReadList<T>(this JsonSerializerOptions options, ref Utf8JsonReader reader, JsonTypeInfo<T> typeInfo)
	{
		if (reader.TokenType == JsonTokenType.Null) return default;

		if (reader.TokenType == JsonTokenType.StartArray)
		{
			var valueConverter = (JsonConverter<T>)typeInfo.Converter;
			var values = new List<T>();
			reader.Read();
			while (reader.TokenType != JsonTokenType.EndArray)
			{
				values.Add(valueConverter.Read(ref reader, typeof(T), options)!);
				reader.Read();
			}

			return values;
		}

		if (reader.TokenType == JsonTokenType.Null) return default;

		throw new JsonException("Expected StartArray or Null");
	}

	/// <summary>
	/// Read and convert the JSON to multiple T.
	/// </summary>
	/// <remarks>
	/// A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON is invalid.
	/// </remarks>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
	/// <param name="typeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	/// <returns>The value that was converted.</returns>
	public static T[]? ReadArray<T>(this JsonSerializerOptions options, ref Utf8JsonReader reader, JsonTypeInfo<T> typeInfo)
	{
		return ReadList(options, ref reader, typeInfo)?.ToArray();
	}

	/// <summary>
	/// Convert and write multiple T to JSON
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
	/// <param name="values">The collection of values to convert.</param>
	/// <param name="typeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	public static void WriteList<T>(this JsonSerializerOptions options, Utf8JsonWriter writer, IEnumerable<T>? values, JsonTypeInfo<T> typeInfo)
	{
		if (values == null)
		{
			writer.WriteNullValue();
			return;
		}

		var valueConverter = (JsonConverter<T>)typeInfo.Converter;
		writer.WriteStartArray();
		foreach (var val in values)
		{
			valueConverter.Write(writer, val, options);
		}
		writer.WriteEndArray();
	}

	/// <summary>
	/// Read and convert string/T dictionary to JSON
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
	/// <param name="valueTypeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	/// <returns>The value that was converted.</returns>
	/// <exception cref="JsonException">The JSON is not valid for this type.</exception>
	public static Dictionary<string, T>? ReadDictionary<T>(this JsonSerializerOptions options, ref Utf8JsonReader reader, JsonTypeInfo<T> valueTypeInfo)
	{
		if (reader.TokenType == JsonTokenType.Null) return default;
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected StartObject or Null");

		var valueConverter = (JsonConverter<T>)valueTypeInfo.Converter;
		var values = new Dictionary<string, T>();
		reader.Read();
		while (reader.TokenType != JsonTokenType.EndObject)
		{
			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException("Expected PropertyName");

			var key = reader.GetString()!;
			reader.Read();
			values[key] = valueConverter.Read(ref reader, typeof(T), options)!;
			reader.Read();
		}

		return values;
	}

	/// <summary>
	/// Read and convert string/list-T dictionary to JSON
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
	/// <param name="valueTypeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	/// <returns>The value that was converted.</returns>
	/// <exception cref="JsonException"></exception>
	public static Dictionary<string, List<T>>? ReadDictionaryList<T>(this JsonSerializerOptions options, ref Utf8JsonReader reader, JsonTypeInfo<T> valueTypeInfo)
	{
		if (reader.TokenType == JsonTokenType.Null) return default;
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected StartObject or Null");

		var values = new Dictionary<string, List<T>>();
		reader.Read();
		while (reader.TokenType != JsonTokenType.EndObject)
		{
			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException("Expected PropertyName");

			var key = reader.GetString()!;
			reader.Read();
			values[key] = options.ReadList(ref reader, valueTypeInfo)!;
			reader.Read();
		}
	
		return values;
	}

	/// <summary>
	/// Convert and write string/T dictionary to JSON
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
	/// <param name="values">The collection of values to convert.</param>
	/// <param name="valueTypeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	public static void WriteDictionary<T>(this JsonSerializerOptions options, Utf8JsonWriter writer,
		IEnumerable<KeyValuePair<string, T>>? values, JsonTypeInfo<T> valueTypeInfo)
	{
		if (values == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartObject();

		var valueConverter = ((JsonConverter<T>)valueTypeInfo.Converter);
		foreach (var entry in values)
		{
			writer.WritePropertyName(entry.Key!);
			valueConverter.Write(writer, entry.Value, options);
		}

		writer.WriteEndObject();
	}

	/// <summary>
	/// Convert and write string/list-T dictionary to JSON
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
	/// <param name="values">The collection of values to convert.</param>
	/// <param name="valueTypeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	public static void WriteDictionaryList<T>(this JsonSerializerOptions options, Utf8JsonWriter writer,
		IEnumerable<KeyValuePair<string, IReadOnlyList<T>>>? values, JsonTypeInfo<T> valueTypeInfo)
	{
		if (values == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartObject();

		foreach (var entry in values)
		{
			writer.WritePropertyName(entry.Key!);
			options.WriteList(writer, entry.Value, valueTypeInfo);
		}

		writer.WriteEndObject();
	}

	/// <summary>
	/// Write an object to JSON. If the type is known, prefer Write<![CDATA[<T>]]>
	/// </summary>
	/// <remarks>
	/// A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON is invalid.
	/// </remarks>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="writer">The <see cref="Utf8JsonReader"/> to read from.</param>
	/// <param name="value">The value to serialize.</param>
	/// <param name="inputType">The type to serialize.</param>
	/// <returns>The value that was converted.</returns>
	[RequiresDynamicCode("Calls JsonSerializer.Serialize. Make sure the options object contains all relevant JsonTypeInfos before suppressing this warning.")]
	[RequiresUnreferencedCode("Calls JsonSerializer.Serialize. Make sure the options object contains all relevant JsonTypeInfos before suppressing this warning.")]
	public static void Write(this JsonSerializerOptions options, Utf8JsonWriter writer, object? value, Type inputType)
	{
		JsonSerializer.Serialize(writer, value, inputType, options);
	}
}
