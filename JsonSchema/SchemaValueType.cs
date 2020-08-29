using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema
{
	/// <summary>
	/// Enumerates the schema value types.
	/// </summary>
	/// <remarks>
	/// This is a flags-enabled enumeration, so values that support multiple
	/// types can be bit-wise OR'd `|` together.
	/// </remarks>
	[Flags]
	[JsonConverter(typeof(EnumStringConverter<SchemaValueType>))]
	public enum SchemaValueType
	{
		/// <summary>
		/// Indicates the value should be an object.
		/// </summary>
		[Description("object")]
		Object = 1 << 0,
		/// <summary>
		/// Indicates the value should be an array.
		/// </summary>
		[Description("array")]
		Array = 1 << 1,
		/// <summary>
		/// Indicates the value should be a boolean.
		/// </summary>
		[Description("boolean")]
		Boolean = 1 << 2,
		/// <summary>
		/// Indicates the value should be a string.
		/// </summary>
		[Description("string")]
		String = 1 << 3,
		/// <summary>
		/// Indicates the value should be a number.
		/// </summary>
		[Description("number")]
		Number = 1 << 4,
		/// <summary>
		/// Indicates the value should be an integer.
		/// </summary>
		[Description("integer")]
		Integer = 1 << 5,
		/// <summary>
		/// Indicates the value should be null.
		/// </summary>
		[Description("null")]
		Null = 1 << 6
	}
}