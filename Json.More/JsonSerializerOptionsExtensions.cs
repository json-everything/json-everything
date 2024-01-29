using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Json.More;

/// <summary>
/// Provides extension functionality for <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
	/// <summary>
	/// Returns the converter for the specified type.
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="typeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	/// <returns>An implementation of <see cref="JsonConverter{T}"/> as determined by the provided options</returns>
	public static JsonConverter<T> GetConverter<T>(this JsonSerializerOptions options, JsonTypeInfo? typeInfo = null)
	{
#if NET8_0_OR_GREATER
		return (JsonConverter<T>)(typeInfo ?? options.GetTypeInfo(typeof(T))).Converter;
#else
		return (JsonConverter<T>)options.GetConverter(typeof(T));
#endif
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
	public static T? Read<T>(this JsonSerializerOptions options, ref Utf8JsonReader reader, JsonTypeInfo<T>? typeInfo = null)
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
	public static void Write<T>(this JsonSerializerOptions options, Utf8JsonWriter writer, T? value, JsonTypeInfo<T>? typeInfo = null)
	{
#pragma warning disable IL2026, IL3050 // This helper is expected to be called with an options object that covers the needed TypeInfos.
		JsonSerializer.Serialize(writer, value, options);
#pragma warning restore IL2026, IL3050
	}

	/// <summary>
	/// Write a T to JSON.
	/// </summary>
	/// <remarks>
	/// A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON is invalid.
	/// </remarks>
	/// <typeparam name="T">The <see cref="Type"/> to convert.</typeparam>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> being used.</param>
	/// <param name="value">The value to serialize.</param>
	/// <param name="typeInfo">An explicit typeInfo to use for looking up the Converter. If not provided, options.GetTypeInfo will be used.</param>
	/// <returns>The value that was converted.</returns>
	public static JsonNode? WriteToNode<T>(this JsonSerializerOptions options, T? value, JsonTypeInfo<T>? typeInfo = null)
	{
#pragma warning disable IL2026, IL3050 // This helper is expected to be called with an options object that covers the needed TypeInfos.
		return JsonSerializer.SerializeToNode(value, options);
#pragma warning restore IL2026, IL3050
	}

}
