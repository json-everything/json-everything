using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Json.More;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Shared reader/writer helpers for the OpenAPI model converters.
/// </summary>
/// <remarks>
/// These support a single forward-only pass over the reader.  Each converter loops
/// over the properties of an object, dispatching on the property name, and collects
/// anything beginning with `x-` as extension data.  Unrecognized keys are captured
/// so that they survive a round trip.
///
/// All serialization routes through <see cref="ApiSerializerContext"/> so that
/// the library remains trim- and AOT-safe.
/// </remarks>
internal static class ConverterHelpers
{
	/// <summary>
	/// Verifies the reader is positioned at the start of an object and advances to the first property.
	/// </summary>
	public static void ExpectObjectStart(this ref Utf8JsonReader reader, string objectType)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected an object for {objectType}");
	}

	/// <summary>
	/// Advances to the next property name in the current object.
	/// </summary>
	/// <returns>The property name, or null when the object has ended.</returns>
	public static string? ReadPropertyName(this ref Utf8JsonReader reader)
	{
		if (!reader.Read())
			throw new JsonException("Unexpected end of JSON");

		if (reader.TokenType == JsonTokenType.EndObject) return null;

		if (reader.TokenType != JsonTokenType.PropertyName)
			throw new JsonException($"Expected a property name but found {reader.TokenType}");

		var name = reader.GetString()!;

		if (!reader.Read())
			throw new JsonException("Unexpected end of JSON");

		return name;
	}

	/// <summary>
	/// Indicates whether a property name denotes extension data.
	/// </summary>
	public static bool IsExtensionKey(string propertyName) =>
		propertyName.StartsWith("x-", StringComparison.Ordinal);

	/// <summary>
	/// Reads the current value as extension data, adding it to <paramref name="extensionData"/>.
	/// </summary>
	public static void ReadExtension(this ref Utf8JsonReader reader, string propertyName, ref ExtensionData? extensionData)
	{
		extensionData ??= [];
		extensionData.Add(propertyName, JsonSerializer.Deserialize(ref reader, ApiSerializerContext.Default.JsonNode));
	}

	/// <summary>
	/// Handles a property that is not recognized: collects it as extension data when it is
	/// `x-` prefixed, otherwise captures it as unknown data.
	/// </summary>
	/// <remarks>
	/// Unrecognized keys are captured rather than rejected, so that documents written against
	/// a later version of the specification still load, and are written back out unchanged.
	/// </remarks>
	public static void ReadUnknown(this ref Utf8JsonReader reader, string propertyName, ref ExtensionData? extensionData, ref UnknownData? unknownData)
	{
		if (IsExtensionKey(propertyName))
		{
			reader.ReadExtension(propertyName, ref extensionData);
			return;
		}

		unknownData ??= [];
		unknownData.Add(propertyName, JsonSerializer.Deserialize(ref reader, ApiSerializerContext.Default.JsonNode));
	}

	/// <summary>
	/// Throws when a required property was absent.
	/// </summary>
	public static T ExpectPresent<T>(T? value, string propertyName, string objectType)
		where T : class =>
		value ?? throw new JsonException($"`{propertyName}` is required for {objectType}.");

	/// <summary>
	/// Reads the current value as a string.
	/// </summary>
	public static string ReadString(this ref Utf8JsonReader reader, string propertyName, string objectType)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException($"`{propertyName}` for {objectType} must be a string");

		return reader.GetString()!;
	}

	/// <summary>
	/// Reads the current value as a URI.
	/// </summary>
	public static Uri ReadUri(this ref Utf8JsonReader reader, string propertyName, string objectType)
	{
		var value = reader.ReadString(propertyName, objectType);

		return new Uri(value, UriKind.RelativeOrAbsolute);
	}

	/// <summary>
	/// Reads the current value as a boolean.
	/// </summary>
	public static bool ReadBool(this ref Utf8JsonReader reader, string propertyName, string objectType)
	{
		if (reader.TokenType is not (JsonTokenType.True or JsonTokenType.False))
			throw new JsonException($"`{propertyName}` for {objectType} must be a boolean");

		return reader.GetBoolean();
	}

	/// <summary>
	/// Reads the current value as an array of strings.
	/// </summary>
	public static List<string> ReadStringArray(this ref Utf8JsonReader reader, string propertyName, string objectType, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException($"`{propertyName}` for {objectType} must be an array");

		return options.ReadList(ref reader, ApiSerializerContext.Default.String)!;
	}

	/// <summary>
	/// Reads the current value as an array.
	/// </summary>
	public static List<T> ReadArray<T>(this ref Utf8JsonReader reader, string propertyName, string objectType, JsonSerializerOptions options, JsonTypeInfo<T> typeInfo)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException($"`{propertyName}` for {objectType} must be an array");

		return options.ReadList(ref reader, typeInfo)!;
	}

	/// <summary>
	/// Reads the current value as an enum.
	/// </summary>
	public static T ReadEnum<T>(this ref Utf8JsonReader reader, string propertyName, string objectType, JsonSerializerOptions options, JsonTypeInfo<T> typeInfo)
		where T : struct, Enum
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException($"`{propertyName}` for {objectType} must be a string");

		return options.Read(ref reader, typeInfo);
	}

	/// <summary>
	/// Reads the current value as a raw JSON node, for user-authored data.
	/// </summary>
	public static JsonNode? ReadNode(this ref Utf8JsonReader reader) =>
		JsonSerializer.Deserialize(ref reader, ApiSerializerContext.Default.JsonNode);

	/// <summary>
	/// Reads the current value as a map of strings.
	/// </summary>
	public static Dictionary<string, string> ReadStringMap(this ref Utf8JsonReader reader, string propertyName, string objectType, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"`{propertyName}` for {objectType} must be an object");

		return options.ReadDictionary(ref reader, ApiSerializerContext.Default.String)!;
	}

	/// <summary>
	/// Reads the current value as a map.
	/// </summary>
	public static Dictionary<string, T> ReadMap<T>(this ref Utf8JsonReader reader, string propertyName, string objectType, JsonSerializerOptions options, JsonTypeInfo<T> typeInfo)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"`{propertyName}` for {objectType} must be an object");

		return options.ReadDictionary(ref reader, typeInfo)!;
	}

	/// <summary>
	/// Writes a property only when the value is present.
	/// </summary>
	public static void MaybeWrite(this Utf8JsonWriter writer, string propertyName, string? value)
	{
		if (value is null) return;

		writer.WriteString(propertyName, value);
	}

	/// <summary>
	/// Writes a property only when the value is present.
	/// </summary>
	public static void MaybeWrite(this Utf8JsonWriter writer, string propertyName, Uri? value)
	{
		if (value is null) return;

		writer.WriteString(propertyName, value.ToString());
	}

	/// <summary>
	/// Writes a property only when the value is present.
	/// </summary>
	public static void MaybeWrite(this Utf8JsonWriter writer, string propertyName, bool? value)
	{
		if (!value.HasValue) return;

		writer.WriteBoolean(propertyName, value.Value);
	}

	/// <summary>
	/// Writes an object property only when the value is present.
	/// </summary>
	public static void MaybeWrite<T>(this Utf8JsonWriter writer, string propertyName, T? value, JsonSerializerOptions options, JsonTypeInfo<T> typeInfo)
		where T : class
	{
		if (value is null) return;

		writer.WritePropertyName(propertyName);
		options.Write(writer, value, typeInfo);
	}

	/// <summary>
	/// Writes an enum property only when the value is present.
	/// </summary>
	public static void MaybeWriteEnum<T>(this Utf8JsonWriter writer, string propertyName, T? value, JsonSerializerOptions options, JsonTypeInfo<T> typeInfo)
		where T : struct, Enum
	{
		if (!value.HasValue) return;

		writer.WritePropertyName(propertyName);
		options.Write(writer, value.Value, typeInfo);
	}

	/// <summary>
	/// Writes a raw JSON node property only when the value is present.
	/// </summary>
	public static void MaybeWriteNode(this Utf8JsonWriter writer, string propertyName, JsonNode? value)
	{
		if (value is null) return;

		writer.WritePropertyName(propertyName);
		JsonSerializer.Serialize(writer, value, ApiSerializerContext.Default.JsonNode);
	}

	/// <summary>
	/// Writes an array property only when the collection is present.
	/// </summary>
	public static void MaybeWriteArray<T>(this Utf8JsonWriter writer, string propertyName, IReadOnlyList<T>? values, JsonSerializerOptions options, JsonTypeInfo<T> typeInfo)
	{
		if (values is null) return;

		writer.WritePropertyName(propertyName);
		options.WriteList(writer, values, typeInfo);
	}

	/// <summary>
	/// Writes an array of strings only when the collection is present.
	/// </summary>
	public static void MaybeWriteStringArray(this Utf8JsonWriter writer, string propertyName, IReadOnlyList<string>? values, JsonSerializerOptions options)
	{
		if (values is null) return;

		writer.WritePropertyName(propertyName);
		options.WriteList(writer, values, ApiSerializerContext.Default.String);
	}

	/// <summary>
	/// Writes a map of strings.
	/// </summary>
	public static void WriteStringMap(this Utf8JsonWriter writer, string propertyName, Dictionary<string, string> values, JsonSerializerOptions options)
	{
		writer.WritePropertyName(propertyName);
		options.WriteDictionary(writer, values, ApiSerializerContext.Default.String);
	}

	/// <summary>
	/// Writes a map property only when the map is present.
	/// </summary>
	public static void MaybeWriteMap<T>(this Utf8JsonWriter writer, string propertyName, Dictionary<string, T>? values, JsonSerializerOptions options, JsonTypeInfo<T> typeInfo)
	{
		if (values is null) return;

		writer.WritePropertyName(propertyName);
		options.WriteDictionary(writer, values, typeInfo);
	}

	/// <summary>
	/// Writes captured unknown data and extension data, if any.
	/// </summary>
	/// <remarks>
	/// Unknown data is written first so that, on a round trip, properties the library does
	/// not recognize are restored alongside the ones it does.
	/// </remarks>
	public static void WriteExtensions(this Utf8JsonWriter writer, ExtensionData? extensionData, UnknownData? unknownData = null)
	{
		if (unknownData is not null)
		{
			foreach (var kvp in unknownData)
			{
				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, ApiSerializerContext.Default.JsonNode!);
			}
		}

		if (extensionData is null) return;

		foreach (var kvp in extensionData)
		{
			writer.WritePropertyName(kvp.Key);
			JsonSerializer.Serialize(writer, kvp.Value, ApiSerializerContext.Default.JsonNode!);
		}
	}
}
