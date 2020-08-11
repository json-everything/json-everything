using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema
{
	[Flags]
	[JsonConverter(typeof(EnumStringConverter<SchemaValueType>))]
	public enum SchemaValueType
	{
		Unknown,
		[Description("object")]
		Object = 1 << 0,
		[Description("array")]
		Array = 1 << 1,
		[Description("boolean")]
		Boolean = 1 << 2,
		[Description("string")]
		String = 1 << 3,
		[Description("number")]
		Number = 1 << 4,
		[Description("integer")]
		Integer = 1 << 5,
		[Description("null")]
		Null = 1 << 6
	}
}