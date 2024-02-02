using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Json.More
{
	/// <summary>
	/// Interface to enable JsonConverter implementations to call other JsonConverter's Read methods
	/// without statically being aware of their type parameters.
	/// </summary>
	public interface IWeaklyTypedJsonConverter
	{
		/// <summary>Reads and converts the JSON to object?.</summary>
		/// <param name="reader">The reader.</param>
		/// <param name="typeToConvert">The type to convert.</param>
		/// <param name="options">An object that specifies serialization options to use.</param>
		/// <returns>The converted value.</returns>
		object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

		/// <summary>Writes a specified value as JSON.</summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="value">The value to convert to JSON.</param>
		/// <param name="options">An object that specifies serialization options to use.</param>
		/// <param name="typeInfo">The <see cref="JsonTypeInfo"/> for the value being serialized.</param>
		void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options, JsonTypeInfo? typeInfo = null);
	}

	/// <summary>
	/// Abstract base class of JsonConverter<typeparamref name="T"/> that helps external code call 
	/// a JsonConverter<typeparamref name="T"/> without statically knowing about T.
	/// </summary>
	public abstract class WeaklyTypedJsonConverter<T> : JsonConverter<T>, IWeaklyTypedJsonConverter
	{
		object? IWeaklyTypedJsonConverter.Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return Read(ref reader, typeToConvert, options);
		}

		void IWeaklyTypedJsonConverter.Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options, JsonTypeInfo? typeInfo)
		{
			if (typeInfo is null)
				Write(writer, (T)value, options);
			else
				options.Write(writer, (T)value, (JsonTypeInfo<T>)typeInfo);
		}
	}

}
