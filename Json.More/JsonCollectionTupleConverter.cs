using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.More;

internal class JsonCollectionTupleConverter : JsonConverterFactory
{
	public static JsonCollectionTupleConverter Instance { get; } = new();

	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.FullName?.StartsWith("System.ValueTuple`") ?? false;
	}

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var typeParams = typeToConvert.GetGenericArguments();
		Type converterType = typeParams.Length switch
		{
			1 => typeof(JsonCollectionTupleConverter<>).MakeGenericType(typeParams),
			2 => typeof(JsonCollectionTupleConverter<,>).MakeGenericType(typeParams),
			3 => typeof(JsonCollectionTupleConverter<,,>).MakeGenericType(typeParams),
			4 => typeof(JsonCollectionTupleConverter<,,,>).MakeGenericType(typeParams),
			5 => typeof(JsonCollectionTupleConverter<,,,,>).MakeGenericType(typeParams),
			6 => typeof(JsonCollectionTupleConverter<,,,,,>).MakeGenericType(typeParams),
			7 => typeof(JsonCollectionTupleConverter<,,,,,,>).MakeGenericType(typeParams),
			8 => typeof(JsonCollectionTupleConverter<,,,,,,,>).MakeGenericType(typeParams),
			_ => throw new ArgumentOutOfRangeException()
		};

		return (JsonConverter)Activator.CreateInstance(converterType, Array.Empty<object>());
	}

	public static JsonSerializerOptions ManageConverters(JsonSerializerOptions options)
	{
		var arrayConverters = options.Converters.OfType<JsonArrayTupleConverter>().ToArray();
		if (arrayConverters.Length != 0)
		{
			options = new JsonSerializerOptions(options);
			// there shouldn't be more than one, but people do weird things
			foreach (var arrayConverter in arrayConverters)
			{
				options.Converters.Remove(arrayConverter);
			}
			options.Converters.Add(Instance);
		}

		return options;
	}
}

internal class JsonCollectionTupleConverter<T> : JsonConverter<ValueTuple<T>>
{
	public override ValueTuple<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = options.Read<T>(ref reader)!;
		reader.Read();

		return new ValueTuple<T>(value);
	}

	public override void Write(Utf8JsonWriter writer, ValueTuple<T> value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Item1, typeof(T), options);
	}
}

internal class JsonCollectionTupleConverter<T1, T2> : JsonConverter<(T1, T2)>
{
	public override (T1, T2) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value1 = options.Read<T1>(ref reader)!;
		reader.Read();
		var value2 = options.Read<T2>(ref reader)!;
		reader.Read();

		return (value1, value2);
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2) value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Item1, typeof(T1), options);
		JsonSerializer.Serialize(writer, value.Item2, typeof(T2), options);
	}
}

internal class JsonCollectionTupleConverter<T1, T2, T3> : JsonConverter<(T1, T2, T3)>
{
	public override (T1, T2, T3) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value1 = options.Read<T1>(ref reader)!;
		reader.Read();
		var value2 = options.Read<T2>(ref reader)!;
		reader.Read();
		var value3 = options.Read<T3>(ref reader)!;
		reader.Read();

		return (value1, value2, value3);
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2, T3) value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Item1, typeof(T1), options);
		JsonSerializer.Serialize(writer, value.Item2, typeof(T2), options);
		JsonSerializer.Serialize(writer, value.Item3, typeof(T3), options);
	}
}

internal class JsonCollectionTupleConverter<T1, T2, T3, T4> : JsonConverter<(T1, T2, T3, T4)>
{
	public override (T1, T2, T3, T4) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value1 = options.Read<T1>(ref reader)!;
		reader.Read();
		var value2 = options.Read<T2>(ref reader)!;
		reader.Read();
		var value3 = options.Read<T3>(ref reader)!;
		reader.Read();
		var value4 = options.Read<T4>(ref reader)!;
		reader.Read();

		return (value1, value2, value3, value4);
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2, T3, T4) value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Item1, typeof(T1), options);
		JsonSerializer.Serialize(writer, value.Item2, typeof(T2), options);
		JsonSerializer.Serialize(writer, value.Item3, typeof(T3), options);
		JsonSerializer.Serialize(writer, value.Item4, typeof(T4), options);
	}
}

internal class JsonCollectionTupleConverter<T1, T2, T3, T4, T5> : JsonConverter<(T1, T2, T3, T4, T5)>
{
	public override (T1, T2, T3, T4, T5) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value1 = options.Read<T1>(ref reader)!;
		reader.Read();
		var value2 = options.Read<T2>(ref reader)!;
		reader.Read();
		var value3 = options.Read<T3>(ref reader)!;
		reader.Read();
		var value4 = options.Read<T4>(ref reader)!;
		reader.Read();
		var value5 = options.Read<T5>(ref reader)!;
		reader.Read();

		return (value1, value2, value3, value4, value5);
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2, T3, T4, T5) value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Item1, typeof(T1), options);
		JsonSerializer.Serialize(writer, value.Item2, typeof(T2), options);
		JsonSerializer.Serialize(writer, value.Item3, typeof(T3), options);
		JsonSerializer.Serialize(writer, value.Item4, typeof(T4), options);
		JsonSerializer.Serialize(writer, value.Item5, typeof(T5), options);
	}
}

internal class JsonCollectionTupleConverter<T1, T2, T3, T4, T5, T6> : JsonConverter<(T1, T2, T3, T4, T5, T6)>
{
	public override (T1, T2, T3, T4, T5, T6) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value1 = options.Read<T1>(ref reader)!;
		reader.Read();
		var value2 = options.Read<T2>(ref reader)!;
		reader.Read();
		var value3 = options.Read<T3>(ref reader)!;
		reader.Read();
		var value4 = options.Read<T4>(ref reader)!;
		reader.Read();
		var value5 = options.Read<T5>(ref reader)!;
		reader.Read();
		var value6 = options.Read<T6>(ref reader)!;
		reader.Read();

		return (value1, value2, value3, value4, value5, value6);
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2, T3, T4, T5, T6) value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Item1, typeof(T1), options);
		JsonSerializer.Serialize(writer, value.Item2, typeof(T2), options);
		JsonSerializer.Serialize(writer, value.Item3, typeof(T3), options);
		JsonSerializer.Serialize(writer, value.Item4, typeof(T4), options);
		JsonSerializer.Serialize(writer, value.Item5, typeof(T5), options);
		JsonSerializer.Serialize(writer, value.Item6, typeof(T6), options);
	}
}

internal class JsonCollectionTupleConverter<T1, T2, T3, T4, T5, T6, T7> : JsonConverter<(T1, T2, T3, T4, T5, T6, T7)>
{
	public override (T1, T2, T3, T4, T5, T6, T7) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value1 = options.Read<T1>(ref reader)!;
		reader.Read();
		var value2 = options.Read<T2>(ref reader)!;
		reader.Read();
		var value3 = options.Read<T3>(ref reader)!;
		reader.Read();
		var value4 = options.Read<T4>(ref reader)!;
		reader.Read();
		var value5 = options.Read<T5>(ref reader)!;
		reader.Read();
		var value6 = options.Read<T6>(ref reader)!;
		reader.Read();
		var value7 = options.Read<T7>(ref reader)!;
		reader.Read();

		return (value1, value2, value3, value4, value5, value6, value7);
	}

	public override void Write(Utf8JsonWriter writer, (T1, T2, T3, T4, T5, T6, T7) value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Item1, typeof(T1), options);
		JsonSerializer.Serialize(writer, value.Item2, typeof(T2), options);
		JsonSerializer.Serialize(writer, value.Item3, typeof(T3), options);
		JsonSerializer.Serialize(writer, value.Item4, typeof(T4), options);
		JsonSerializer.Serialize(writer, value.Item5, typeof(T5), options);
		JsonSerializer.Serialize(writer, value.Item6, typeof(T6), options);
		JsonSerializer.Serialize(writer, value.Item7, typeof(T7), options);
	}
}

internal class JsonCollectionTupleConverter<T1, T2, T3, T4, T5, T6, T7, TRest> : JsonConverter<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>
	where TRest : struct
{
	public override ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value1 = options.Read<T1>(ref reader)!;
		reader.Read();
		var value2 = options.Read<T2>(ref reader)!;
		reader.Read();
		var value3 = options.Read<T3>(ref reader)!;
		reader.Read();
		var value4 = options.Read<T4>(ref reader)!;
		reader.Read();
		var value5 = options.Read<T5>(ref reader)!;
		reader.Read();
		var value6 = options.Read<T6>(ref reader)!;
		reader.Read();
		var value7 = options.Read<T7>(ref reader)!;
		reader.Read();
		var rest = options.Read<TRest>(ref reader);
		reader.Read();

		return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(value1, value2, value3, value4, value5, value6, value7, rest);
	}

	public override void Write(Utf8JsonWriter writer, ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Item1, typeof(T1), options);
		JsonSerializer.Serialize(writer, value.Item2, typeof(T2), options);
		JsonSerializer.Serialize(writer, value.Item3, typeof(T3), options);
		JsonSerializer.Serialize(writer, value.Item4, typeof(T4), options);
		JsonSerializer.Serialize(writer, value.Item5, typeof(T5), options);
		JsonSerializer.Serialize(writer, value.Item6, typeof(T6), options);
		JsonSerializer.Serialize(writer, value.Item7, typeof(T7), options);
		JsonSerializer.Serialize(writer, value.Rest, typeof(TRest), options);
	}
}
