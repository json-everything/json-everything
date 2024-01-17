using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.More;

/// <summary>
/// Provides JSON serialization for the <see cref="ValueTuple{T1}"/> family of types.
/// </summary>
public class JsonArrayTupleConverter : JsonConverterFactory
{
	/// <summary>When overridden in a derived class, determines whether the converter instance can convert the specified object type.</summary>
	/// <param name="typeToConvert">The type of the object to check whether it can be converted by this converter instance.</param>
	/// <returns>
	/// <see langword="true" /> if the instance can convert the specified object type; otherwise, <see langword="false" />.</returns>
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.FullName?.StartsWith("System.ValueTuple`") ?? false;
	}

	/// <summary>Creates a converter for a specified type.</summary>
	/// <param name="typeToConvert">The type handled by the converter.</param>
	/// <param name="options">The serialization options to use.</param>
	/// <returns>A converter for which <typeparamref name="T" /> is compatible with <paramref name="typeToConvert" />.</returns>
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var typeParams = typeToConvert.GetGenericArguments();
		Type converterType = typeParams.Length switch
		{
			1 => typeof(JsonArrayTupleConverter<>).MakeGenericType(typeParams),
			2 => typeof(JsonArrayTupleConverter<,>).MakeGenericType(typeParams),
			3 => typeof(JsonArrayTupleConverter<,,>).MakeGenericType(typeParams),
			4 => typeof(JsonArrayTupleConverter<,,,>).MakeGenericType(typeParams),
			5 => typeof(JsonArrayTupleConverter<,,,,>).MakeGenericType(typeParams),
			6 => typeof(JsonArrayTupleConverter<,,,,,>).MakeGenericType(typeParams),
			7 => typeof(JsonArrayTupleConverter<,,,,,,>).MakeGenericType(typeParams),
			8 => typeof(JsonArrayTupleConverter<,,,,,,,>).MakeGenericType(typeParams),
			_ => throw new ArgumentOutOfRangeException()
		};

		return (JsonConverter)Activator.CreateInstance(converterType, Array.Empty<object>());
	}
}

internal class JsonArrayTupleConverter<T> : JsonConverter<ValueTuple<T>>
{
	public override ValueTuple<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected tuple to be encoded as an array.");
		reader.Read();

		options = JsonCollectionTupleConverter.ManageConverters(options);

		var value = options.Read<ValueTuple<T>>(ref reader)!;
		reader.Read();

		if (reader.TokenType != JsonTokenType.EndArray)
			throw new JsonException("Expected end of array.");
		reader.Read();

		return value;
	}

	public override void Write(Utf8JsonWriter writer, ValueTuple<T> value, JsonSerializerOptions options)
	{
		options = JsonCollectionTupleConverter.ManageConverters(options);

		writer.WriteStartArray();
		JsonSerializer.Serialize(writer, value, typeof(ValueTuple<T>), options);
		writer.WriteEndArray();
	}
}

internal class JsonArrayTupleConverter<T1, T2> : JsonConverter<(T1, T2)>
{
	public override (T1, T2) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected tuple to be encoded as an array.");
		reader.Read();

		options = JsonCollectionTupleConverter.ManageConverters(options);

		var value = options.Read<(T1, T2)>(ref reader)!;
		reader.Read();

		if (reader.TokenType != JsonTokenType.EndArray)
			throw new JsonException("Expected end of array.");
		reader.Read();

		return value;
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2) value, JsonSerializerOptions options)
	{
		options = JsonCollectionTupleConverter.ManageConverters(options);

		writer.WriteStartArray();
		JsonSerializer.Serialize(writer, value, typeof((T1, T2)), options);
		writer.WriteEndArray();
	}
}

internal class JsonArrayTupleConverter<T1, T2, T3> : JsonConverter<(T1, T2, T3)>
{
	public override (T1, T2, T3) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected tuple to be encoded as an array.");
		reader.Read();

		options = JsonCollectionTupleConverter.ManageConverters(options);

		var value = options.Read<(T1, T2, T3)>(ref reader)!;
		reader.Read();

		if (reader.TokenType != JsonTokenType.EndArray)
			throw new JsonException("Expected end of array.");
		reader.Read();

		return value;
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2, T3) value, JsonSerializerOptions options)
	{
		options = JsonCollectionTupleConverter.ManageConverters(options);

		writer.WriteStartArray();
		JsonSerializer.Serialize(writer, value, typeof((T1, T2, T3)), options);
		writer.WriteEndArray();
	}
}

internal class JsonArrayTupleConverter<T1, T2, T3, T4> : JsonConverter<(T1, T2, T3, T4)>
{
	public override (T1, T2, T3, T4) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected tuple to be encoded as an array.");
		reader.Read();

		options = JsonCollectionTupleConverter.ManageConverters(options);

		var value = options.Read<(T1, T2, T3, T4)>(ref reader)!;
		reader.Read();

		if (reader.TokenType != JsonTokenType.EndArray)
			throw new JsonException("Expected end of array.");
		reader.Read();

		return value;
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2, T3, T4) value, JsonSerializerOptions options)
	{
		options = JsonCollectionTupleConverter.ManageConverters(options);

		writer.WriteStartArray();
		JsonSerializer.Serialize(writer, value, typeof((T1, T2, T3, T4)), options);
		writer.WriteEndArray();
	}
}

internal class JsonArrayTupleConverter<T1, T2, T3, T4, T5> : JsonConverter<(T1, T2, T3, T4, T5)>
{
	public override (T1, T2, T3, T4, T5) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected tuple to be encoded as an array.");
		reader.Read();

		options = JsonCollectionTupleConverter.ManageConverters(options);

		var value = options.Read<(T1, T2, T3, T4, T5)>(ref reader)!;
		reader.Read();

		if (reader.TokenType != JsonTokenType.EndArray)
			throw new JsonException("Expected end of array.");
		reader.Read();

		return value;
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2, T3, T4, T5) value, JsonSerializerOptions options)
	{
		options = JsonCollectionTupleConverter.ManageConverters(options);

		writer.WriteStartArray();
		JsonSerializer.Serialize(writer, value, typeof((T1, T2, T3, T4, T5)), options);
		writer.WriteEndArray();
	}
}

internal class JsonArrayTupleConverter<T1, T2, T3, T4, T5, T6> : JsonConverter<(T1, T2, T3, T4, T5, T6)>
{
	public override (T1, T2, T3, T4, T5, T6) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected tuple to be encoded as an array.");
		reader.Read();

		options = JsonCollectionTupleConverter.ManageConverters(options);

		var value = options.Read<(T1, T2, T3, T4, T5, T6)>(ref reader)!;
		reader.Read();

		if (reader.TokenType != JsonTokenType.EndArray)
			throw new JsonException("Expected end of array.");
		reader.Read();

		return value;
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2, T3, T4, T5, T6) value, JsonSerializerOptions options)
	{
		options = JsonCollectionTupleConverter.ManageConverters(options);

		writer.WriteStartArray();
		JsonSerializer.Serialize(writer, value, typeof((T1, T2, T3, T4, T5, T6)), options);
		writer.WriteEndArray();
	}
}

internal class JsonArrayTupleConverter<T1, T2, T3, T4, T5, T6, T7> : JsonConverter<(T1, T2, T3, T4, T5, T6, T7)>
{
	public override (T1, T2, T3, T4, T5, T6, T7) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected tuple to be encoded as an array.");
		reader.Read();

		options = JsonCollectionTupleConverter.ManageConverters(options);

		var value = options.Read<(T1, T2, T3, T4, T5, T6, T7)>(ref reader)!;
		reader.Read();

		if (reader.TokenType != JsonTokenType.EndArray)
			throw new JsonException("Expected end of array.");
		reader.Read();

		return value;
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2, T3, T4, T5, T6, T7) value, JsonSerializerOptions options)
	{
		options = JsonCollectionTupleConverter.ManageConverters(options);

		writer.WriteStartArray();
		JsonSerializer.Serialize(writer, value, typeof((T1, T2, T3, T4, T5, T6, T7)), options);
		writer.WriteEndArray();
	}
}

internal class JsonArrayTupleConverter<T1, T2, T3, T4, T5, T6, T7, TRest> : JsonConverter<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>
	where TRest : struct
{
	public override ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected tuple to be encoded as an array.");
		reader.Read();

		options = JsonCollectionTupleConverter.ManageConverters(options);

		var value = options.Read<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>(ref reader)!;
		reader.Read();

		if (reader.TokenType != JsonTokenType.EndArray)
			throw new JsonException("Expected end of array.");
		reader.Read();

		return value;
	}

	public override void Write(Utf8JsonWriter writer, ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> value, JsonSerializerOptions options)
	{
		options = JsonCollectionTupleConverter.ManageConverters(options);
		
		writer.WriteStartArray();
		JsonSerializer.Serialize(writer, value, typeof(ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>), options);
		writer.WriteEndArray();
	}
}
