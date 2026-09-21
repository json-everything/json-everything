using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.More;

/// <summary>
/// Provides extension functionality for <see cref="JsonElement"/>.
/// </summary>
public static class JsonElementExtensions
{
	/// <summary>
	/// Determines JSON-compatible equivalence.
	/// </summary>
	/// <param name="a">The first element.</param>
	/// <param name="b">The second element.</param>
	/// <returns>`true` if the element are equivalent; `false` otherwise.</returns>
	/// <exception cref="ArgumentOutOfRangeException">The <see cref="JsonElement.ValueKind"/> is not valid.</exception>
	public static bool IsEquivalentTo(this JsonElement a, JsonElement b)
	{
		if (a.ValueKind != b.ValueKind) return false;
		switch (a.ValueKind)
		{
			case JsonValueKind.Object:
				var aProperties = a.EnumerateObject().ToList();
				var bProperties = b.EnumerateObject().ToList();
				if (aProperties.Count != bProperties.Count) return false;
				var grouped = aProperties.Concat(bProperties)
					.GroupBy(p => p.Name)
					.Select(g => g.ToList())
					.ToList();
				return grouped.All(g => g.Count == 2 && g[0].Value.IsEquivalentTo(g[1].Value));
			case JsonValueKind.Array:
				var aElements = a.EnumerateArray().ToList();
				var bElements = b.EnumerateArray().ToList();
				if (aElements.Count != bElements.Count) return false;
				var zipped = aElements.Zip(bElements, (ae, be) => (ae, be));
				return zipped.All(p => p.ae.IsEquivalentTo(p.be));
			case JsonValueKind.String:
				return a.GetString() == b.GetString();
			case JsonValueKind.Number:
				return a.GetDecimal() == b.GetDecimal();
			case JsonValueKind.Undefined:
				return false;
			case JsonValueKind.True:
			case JsonValueKind.False:
			case JsonValueKind.Null:
				return true;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	// source: https://stackoverflow.com/a/60592310/878701, modified for netstandard2.0
	// license: https://creativecommons.org/licenses/by-sa/4.0/
	/// <summary>
	/// Generate a consistent JSON-value-based hash code for the element.
	/// </summary>
	/// <param name="element">The element.</param>
	/// <param name="maxHashDepth">Maximum depth to calculate.  Default is -1 which utilizes the entire structure without limitation.</param>
	/// <returns>The hash code.</returns>
	/// <remarks>
	/// See the following for discussion on why the default implementation is insufficient:
	///
	/// - https://github.com/json-everything/json-everything/issues/76
	/// - https://github.com/dotnet/runtime/issues/33388
	/// </remarks>
	public static int GetEquivalenceHashCode(this JsonElement element, int maxHashDepth = -1)
	{
		static void Add(ref int current, object? newValue)
		{
			unchecked
			{
				current = current * 397 ^ (newValue?.GetHashCode() ?? 0);
			}
		}

		void ComputeHashCode(JsonElement obj, ref int current, int depth)
		{
			Add(ref current, obj.ValueKind);

			switch (obj.ValueKind)
			{
				case JsonValueKind.Null:
				case JsonValueKind.True:
				case JsonValueKind.False:
				case JsonValueKind.Undefined:
					break;

				case JsonValueKind.Number:
					Add(ref current, obj.GetRawText());
					break;

				case JsonValueKind.String:
					Add(ref current, obj.GetString());
					break;

				case JsonValueKind.Array:
					if (depth != maxHashDepth)
						foreach (var item in obj.EnumerateArray())
							ComputeHashCode(item, ref current, depth + 1);
					else
						Add(ref current, obj.GetArrayLength());
					break;

				case JsonValueKind.Object:
					foreach (var property in obj.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
					{
						Add(ref current, property.Name);
						if (depth != maxHashDepth)
							ComputeHashCode(property.Value, ref current, depth + 1);
					}
					break;

				default:
					throw new JsonException($"Unknown JsonValueKind {obj.ValueKind}");
			}
		}

		var hash = 0;
		ComputeHashCode(element, ref hash, 0);
		return hash;

	}

	/// <summary>
	/// Just a shortcut for calling `JsonSerializer.Serialize()` because `.ToString()` doesn't do what you might expect.
	/// </summary>
	/// <param name="element">The value to convert.</param>
	/// <returns>A JSON string.</returns>
	/// <remarks>
	/// See https://github.com/dotnet/runtime/issues/42502
	/// </remarks>
	public static string ToJsonString(this JsonElement element)
	{
		return JsonSerializer.Serialize(element, MoreSerializerContext.Default.JsonElement);
	}

	/// <summary>
	/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
	/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
	public static JsonElement AsJsonElement(this long value)
	{
		using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
		return doc.RootElement.Clone();
	}

	/// <summary>
	/// Converts a <see cref="int"/> to a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
	/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
	public static JsonElement AsJsonElement(this int value)
	{
		using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
		return doc.RootElement.Clone();
	}

	/// <summary>
	/// Converts a <see cref="short"/> to a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
	/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
	public static JsonElement AsJsonElement(this short value)
	{
		using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
		return doc.RootElement.Clone();
	}

	/// <summary>
	/// Converts a <see cref="bool"/> to a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
	/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
	public static JsonElement AsJsonElement(this bool value)
	{
		using var doc = JsonDocument.Parse(value ? "true" : "false");
		return doc.RootElement.Clone();
	}

	/// <summary>
	/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
	/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
	public static JsonElement AsJsonElement(this decimal value)
	{
		using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
		return doc.RootElement.Clone();
	}

	/// <summary>
	/// Converts a <see cref="double"/> to a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
	/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
	public static JsonElement AsJsonElement(this double value)
	{
		using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
		return doc.RootElement.Clone();
	}

	/// <summary>
	/// Converts a <see cref="float"/> to a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
	/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
	public static JsonElement AsJsonElement(this float value)
	{
		using var doc = JsonDocument.Parse(value.ToString(CultureInfo.InvariantCulture));
		return doc.RootElement.Clone();
	}

	/// <summary>
	/// Converts a <see cref="string"/> to a <see cref="JsonElement"/>.  Can also be used to get a `null` element.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
	/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
	public static JsonElement AsJsonElement(this string? value)
	{
		using var doc = JsonDocument.Parse(JsonSerializer.Serialize(value, MoreSerializerContext.Default.String!));
		return doc.RootElement.Clone();
	}

	/// <summary>
	/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="values">The array of values to convert.</param>
	/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
	/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
	public static JsonElement AsJsonElement(this IEnumerable<JsonElement> values)
	{
		using var doc = JsonDocument.Parse($"[{string.Join(",", values.Select(v => v.ToJsonString()))}]");
		return doc.RootElement.Clone();
	}

	/// <summary>
	/// Converts a <see cref="long"/> to a <see cref="JsonElement"/>.
	/// </summary>
	/// <param name="values">The value to convert.</param>
	/// <returns>A <see cref="JsonElement"/> representing the value.</returns>
	/// <remarks>This is a workaround for lack of native support in the System.Text.Json namespace.</remarks>
	public static JsonElement AsJsonElement(this IDictionary<string, JsonElement> values)
	{
		using var doc = JsonDocument.Parse($"{{{string.Join(",", values.Select(v => $"{JsonSerializer.Serialize(v.Key, MoreSerializerContext.Default.String)}:{v.Value.ToJsonString()}"))}}}");
		return doc.RootElement.Clone();
	}

	/// <summary>
	/// Converts a <see cref="JsonElement"/> to a <see cref="JsonNode"/>.
	/// </summary>
	/// <param name="element">The element.</param>
	/// <returns>An equivalent node.</returns>
	/// <remarks>
	/// This provides a single point of conversion as one is not provided by .Net.
	/// See https://github.com/dotnet/runtime/issues/70427 for more information.
	/// </remarks>
	public static JsonNode? AsNode(this JsonElement element) => element.ValueKind switch
	{
		JsonValueKind.Array => JsonArray.Create(element),
		JsonValueKind.Object => JsonObject.Create(element),
		_ => JsonValue.Create(element)
	};

	[RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(JsonSerializerOptions)")]
	[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(JsonSerializerOptions)")]
	internal static T? ReadValue<T>(ref JsonElement.ArrayEnumerator enumerator, JsonSerializerOptions options)
	{
		enumerator.MoveNext();
		return enumerator.Current.Deserialize<T>(options);
	}

	/// <summary>
	/// Defines a contract for analyzing a byte array to produce a value.
	/// </summary>
	/// <typeparam name="T">The return type</typeparam>
	/// <param name="data">The byte array</param>
	/// <returns>The analysis result</returns>
	public delegate T ByteAnalyzer<out T>(ReadOnlySpan<byte> data);

	/// <summary>
	/// Provides a means to analyze the raw data of a <see cref="JsonElement"/>.
	/// </summary>
	/// <typeparam name="T">The output type of the analysis</typeparam>
	/// <param name="element">The <see cref="JsonElement"/></param>
	/// <param name="analyze">The function that performs the analysis</param>
	/// <returns>The analysis result</returns>
	public static T AnalyzeRawBytes<T>(this JsonElement element, ByteAnalyzer<T> analyze)
	{
		ReadOnlySpan<byte> span;
#if NET9_0_OR_GREATER
		span = JsonMarshal.GetRawUtf8Value(element);
#else
		using var buffer = new PooledBufferWriter();
		using (var writer = new Utf8JsonWriter(buffer))
		{
			element.WriteTo(writer);
		}
		span = buffer.WrittenSpan;
#endif
		return analyze(span);
	}

#if !NET9_0_OR_GREATER
	private class PooledBufferWriter : IBufferWriter<byte>, IDisposable
	{
		private byte[] _buffer;
		private int _index;

		public PooledBufferWriter(int initialCapacity = 256)
		{
			_buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
		}

		public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, _index);

		public void Advance(int count) => _index += count;

		public Memory<byte> GetMemory(int sizeHint = 0)
		{
			CheckAndResizeBuffer(sizeHint);
			return _buffer.AsMemory(_index);
		}

		public Span<byte> GetSpan(int sizeHint = 0)
		{
			CheckAndResizeBuffer(sizeHint);
			return _buffer.AsSpan(_index);
		}

		private void CheckAndResizeBuffer(int sizeHint)
		{
			var needed = _index + (sizeHint > 0 ? sizeHint : 1);
			if (needed <= _buffer.Length) return;

			var newSize = Math.Max(_buffer.Length * 2, needed);
			var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
			Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _index);
			ArrayPool<byte>.Shared.Return(_buffer);
			_buffer = newBuffer;
		}

		public void Dispose()
		{
			if (_buffer == null!) return;

			ArrayPool<byte>.Shared.Return(_buffer);
			_buffer = null!;
		}
	}
#endif

	/// <summary>
	/// Compares the string value of a <see cref="JsonElement"/> against a target string 
	/// by indexing and decoding the raw JSON bytes on the fly, ensuring zero heap allocations.
	/// </summary>
	public static bool EqualsString(this JsonElement element, string target)
	{
		if (element.ValueKind != JsonValueKind.String) return false;

		return element.AnalyzeRawBytes(rawJsonSpan =>
		{
			if (rawJsonSpan.Length < 2 || rawJsonSpan[0] != (byte)'"' || rawJsonSpan[^1] != (byte)'"')
				return false;

			var innerJsonSpan = rawJsonSpan.Slice(1, rawJsonSpan.Length - 2);

			return CompareJsonBytesToUtf16String(innerJsonSpan, target);
		});
	}

	private static bool CompareJsonBytesToUtf16String(ReadOnlySpan<byte> jsonBytes, string target)
	{
		var byteIdx = 0;
		var targetIdx = 0;

		while (byteIdx < jsonBytes.Length)
		{
			if (targetIdx >= target.Length) return false;

			var b1 = jsonBytes[byteIdx++];

			int scalar;

			switch (b1)
			{
				case (byte)'\\' when byteIdx >= jsonBytes.Length:
					return false;
				case (byte)'\\':
				{
					var escapeToken = jsonBytes[byteIdx++];

					switch (escapeToken)
					{
						case (byte)'"': scalar = '"'; break;
						case (byte)'\\': scalar = '\\'; break;
						case (byte)'/': scalar = '/'; break;
						case (byte)'b': scalar = '\b'; break;
						case (byte)'f': scalar = '\f'; break;
						case (byte)'n': scalar = '\n'; break;
						case (byte)'r': scalar = '\r'; break;
						case (byte)'t': scalar = '\t'; break;
						case (byte)'u':
							if (byteIdx + 4 > jsonBytes.Length) return false;
							if (!TryParseHex4(jsonBytes.Slice(byteIdx, 4), out scalar)) return false;
							byteIdx += 4;

							if (scalar is >= 0xD800 and <= 0xDBFF)
							{
								if (byteIdx + 6 > jsonBytes.Length ||
								    jsonBytes[byteIdx] != (byte)'\\' ||
								    jsonBytes[byteIdx + 1] != (byte)'u')
								{
									return false;
								}
								if (!TryParseHex4(jsonBytes.Slice(byteIdx + 2, 4), out var lowSurrogate)) return false;
								byteIdx += 6;

								if (targetIdx + 1 >= target.Length ||
								    target[targetIdx++] != (char)scalar ||
								    target[targetIdx++] != (char)lowSurrogate)
								{
									return false;
								}
								continue;
							}
							break;
						default:
							return false;
					}

					break;
				}
				case <= 0x7F:
					scalar = b1;
					break;
				default:
				{
					if ((b1 & 0xE0) == 0xC0)
					{
						if (byteIdx >= jsonBytes.Length) return false;

						var b2 = jsonBytes[byteIdx++];
						scalar = ((b1 & 0x1F) << 6) | (b2 & 0x3F);
					}
					else if ((b1 & 0xF0) == 0xE0)
					{
						if (byteIdx + 1 >= jsonBytes.Length) return false;

						var b2 = jsonBytes[byteIdx++];
						var b3 = jsonBytes[byteIdx++];
						scalar = ((b1 & 0x0F) << 12) | ((b2 & 0x3F) << 6) | (b3 & 0x3F);
					}
					else if ((b1 & 0xF8) == 0xF0)
					{
						if (byteIdx + 2 >= jsonBytes.Length) return false;

						var b2 = jsonBytes[byteIdx++];
						var b3 = jsonBytes[byteIdx++];
						var b4 = jsonBytes[byteIdx++];

						scalar = ((b1 & 0x07) << 18) | ((b2 & 0x3F) << 12) | ((b3 & 0x3F) << 6) | (b4 & 0x3F);

						scalar -= 0x10000;
						var highChar = (char)((scalar >> 10) + 0xD800);
						var lowChar = (char)((scalar & 0x3FF) + 0xDC00);

						if (targetIdx + 1 >= target.Length || target[targetIdx++] != highChar || target[targetIdx++] != lowChar)
							return false;
						continue;
					}
					else
						return false;

					break;
				}
			}

			if (target[targetIdx++] != (char)scalar) return false;
		}

		return targetIdx == target.Length;
	}

	private static bool TryParseHex4(ReadOnlySpan<byte> bytes, out int value)
	{
		value = 0;
		for (int i = 0; i < 4; i++)
		{
			var b = bytes[i];
			int v;
			if (b >= '0' && b <= '9') v = b - '0';
			else if (b >= 'a' && b <= 'f') v = b - 'a' + 10;
			else if (b >= 'A' && b <= 'F') v = b - 'A' + 10;
			else return false;

			value = (value << 4) | v;
		}
		return true;
	}
}

[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(string))]
internal partial class MoreSerializerContext : JsonSerializerContext;