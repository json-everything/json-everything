using System;
using System.Numerics;
using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Provides some schema-related functionality for <see cref="JsonElement"/>.
/// </summary>
public static class JsonElementExtensions
{
	/// <summary>
	/// Provides the JSON Schema type for a value.
	/// </summary>
	/// <param name="element">The node.</param>
	/// <returns>The <see cref="SchemaValueType"/> of the value.</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public static SchemaValueType GetSchemaValueType(this JsonElement element) =>
		element.ValueKind switch
		{
			JsonValueKind.Object => SchemaValueType.Object,
			JsonValueKind.Array => SchemaValueType.Array,
			JsonValueKind.String => SchemaValueType.String,
			JsonValueKind.Number => element.TryGetInt64(out _)
				? SchemaValueType.Integer
				: SchemaValueType.Number,
			JsonValueKind.True => SchemaValueType.Boolean,
			JsonValueKind.False => SchemaValueType.Boolean,
			JsonValueKind.Null => SchemaValueType.Null,
			_ => throw new ArgumentOutOfRangeException(nameof(element.ValueKind), element.ValueKind, null)
		};
}

public readonly struct JsonNumber
{
	private readonly IntegerProcessing? _integerProcessing;
	private readonly NumberProcessing? _numberProcessing;

	public int? Int32 { get; }
	public long? Int64 { get; }
#if NET8_0_OR_GREATER
	public Int128? Int128 { get; }
#endif
	public BigInteger? BigInt { get; }

	public double? Double { get; }
	public decimal? Decimal { get; }

	public JsonNumber(JsonElement value, NumberProcessing numberProcessing)
	{
		_numberProcessing = numberProcessing;

		switch (numberProcessing)
		{
			case NumberProcessing.Double:
				Double = value.GetDouble();
				break;
			case NumberProcessing.Decimal:
				Decimal = value.GetDecimal();
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(numberProcessing), numberProcessing, null);
		}
	}

	public JsonNumber(JsonElement value, IntegerProcessing integerProcessing)
	{
		_integerProcessing = integerProcessing;

		switch (integerProcessing)
		{
			case IntegerProcessing.Int32:
				Int32 = value.GetInt32();
				break;
			case IntegerProcessing.Int64:
				Int64 = value.GetInt64();
				break;
#if NET8_0_OR_GREATER
			//case IntegerProcessing.Int128:
			//	Int128 = Int128.Parse(value.GetString()!);
			//	break;
#endif
			case IntegerProcessing.BigInt:
				BigInt = BigInteger.Parse(value.GetString()!);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(integerProcessing), integerProcessing, null);
		}
	}

	//public static bool operator ==(JsonNumber a, JsonNumber b)
	//{
	//	if (a.Int32.HasValue && a.Int32 == b.Int32) return true;
	//	if (a.Int128.HasValue && a.Int128 == b.Int128) return true;
	//}
}