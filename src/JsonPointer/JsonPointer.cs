using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Pointer;

/// <summary>
/// Represents a JSON Pointer as defined in RFC 6901.
/// This implementation is optimized for minimal allocations.
/// </summary>
[JsonConverter(typeof(JsonPointerJsonConverter))]
[TypeConverter(typeof(JsonPointerTypeConverter))]
public readonly struct JsonPointer : IEquatable<JsonPointer>
{
	/// <summary>
	/// Represents an empty JSON Pointer.
	/// </summary>
	public static readonly JsonPointer Empty = new(ReadOnlyMemory<char>.Empty, 0);

	private readonly ReadOnlyMemory<char> _pointer;
	private readonly int _segmentCount;

	private JsonPointer(ReadOnlyMemory<char> pointer, int segmentCount)
	{
		_pointer = pointer;
		_segmentCount = segmentCount;
	}

	/// <summary>
	/// Creates a new JSON Pointer from a string.
	/// </summary>
	/// <param name="pointer">The JSON Pointer string (e.g., "/foo/0/bar")</param>
	/// <returns>A new JsonPointer instance</returns>
	public static JsonPointer Parse(string pointer)
	{
		if (pointer == null)
			throw new ArgumentNullException(nameof(pointer));

		if (pointer.Length == 0)
			return Empty;

		if (pointer[0] != '/')
			throw new FormatException("JSON PointerOld must start with '/'");

		int segmentCount = 0;
		for (int i = 0; i < pointer.Length; i++)
		{
			if (pointer[i] == '/')
				segmentCount++;
		}

		return new JsonPointer(pointer.AsMemory(), segmentCount);
	}

	/// <summary>
	/// Attempts to parse a JSON PointerOld from a string.
	/// </summary>
	/// <param name="pointer">The JSON PointerOld string to parse.</param>
	/// <param name="result">The parsed pointerOld if successful; null otherwise.</param>
	/// <returns>True if parsing was successful; false otherwise.</returns>
	public static bool TryParse(string? pointer, out JsonPointer result)
	{
		if (pointer == null)
		{
			result = default;
			return false;
		}

		if (pointer.Length == 0)
		{
			result = Empty;
			return true;
		}

		if (pointer[0] != '/')
		{
			result = default;
			return false;
		}

		int segmentCount = 0;
		for (int i = 0; i < pointer.Length; i++)
		{
			if (pointer[i] == '/')
				segmentCount++;
		}

		result = new JsonPointer(pointer.AsMemory(), segmentCount);
		return true;
	}

	/// <summary>
	/// Gets the number of segments in the pointerOld.
	/// </summary>
	public int SegmentCount => _segmentCount;

	/// <summary>
	/// Gets the parent pointerOld of this pointerOld.
	/// </summary>
	/// <returns>The parent pointerOld, or null if this is the root pointerOld</returns>
	public JsonPointer? GetParent()
	{
		if (_segmentCount <= 1)
			return null;

		var span = _pointer.Span;
		int lastSlash = span.LastIndexOf('/');
		if (lastSlash <= 0)
			return null;

		return new JsonPointer(_pointer[..lastSlash], _segmentCount - 1);
	}

	/// <summary>
	/// Combines this pointerOld with another pointerOld.
	/// </summary>
	/// <param name="other">The pointerOld to append</param>
	/// <returns>A new pointerOld representing the combination</returns>
	public JsonPointer Combine(JsonPointer other)
	{
		if (other._pointer.IsEmpty)
			return this;

		if (_pointer.IsEmpty)
			return other;

		int totalLength = _pointer.Length + other._pointer.Length;
            
		// Use stackalloc for small pointers (up to 256 chars)
		if (totalLength <= 256)
		{
			Span<char> combined = stackalloc char[totalLength];
			_pointer.Span.CopyTo(combined);
			other._pointer.Span.CopyTo(combined[_pointer.Length..]);
			return new JsonPointer(combined.ToArray(), _segmentCount + other._segmentCount);
		}
            
		// Use ArrayPool for larger pointers
		char[]? rented = null;
		try
		{
			rented = ArrayPool<char>.Shared.Rent(totalLength);
			var combined = rented.AsSpan(0, totalLength);
			_pointer.Span.CopyTo(combined);
			other._pointer.Span.CopyTo(combined[_pointer.Length..]);
			return new JsonPointer(combined.ToArray(), _segmentCount + other._segmentCount);
		}
		finally
		{
			if (rented != null)
			{
				ArrayPool<char>.Shared.Return(rented);
			}
		}
	}

	/// <summary>
	/// Gets all ancestor pointers of this pointerOld.
	/// </summary>
	/// <returns>An enumerable of all ancestor pointers</returns>
	public IEnumerable<JsonPointer> GetAncestors()
	{
		var current = this;
		while (current.GetParent() is { } parent)
		{
			yield return parent;
			current = parent;
		}
	}

	/// <summary>
	/// Gets the string representation of this pointerOld.
	/// </summary>
	/// <returns>The pointerOld string</returns>
	public override string ToString() => _pointer.ToString();

	/// <summary>
	/// Gets a segment from the pointerOld by index.
	/// </summary>
	/// <param name="index">The zero-based index of the segment</param>
	/// <returns>The segment as a ReadOnlyMemory&lt;char&gt;</returns>
	public ReadOnlyMemory<char> GetSegment(int index)
	{
		if (index < 0 || index >= _segmentCount)
			throw new ArgumentOutOfRangeException(nameof(index));

		var span = _pointer.Span;
		int start = 0;
		int currentIndex = 0;

		for (int i = 0; i < span.Length; i++)
		{
			if (span[i] == '/')
			{
				if (currentIndex == index)
				{
					start = i + 1;
				}
				else if (currentIndex == index + 1)
				{
					return _pointer[start..i];
				}
				currentIndex++;
			}
		}

		return _pointer[start..];
	}

	/// <summary>
	/// Decodes a JSON PointerOld segment by replacing escape sequences.
	/// </summary>
	/// <param name="segment">The segment to decode</param>
	/// <returns>The decoded segment</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string DecodeSegment(ReadOnlySpan<char> segment)
	{
		if (segment.IsEmpty)
			return string.Empty;

		int tildeIndex = segment.IndexOf('~');
		if (tildeIndex == -1)
			return segment.ToString();

		var result = new char[segment.Length];
		int writeIndex = 0;

		for (int i = 0; i < segment.Length; i++)
		{
			if (segment[i] == '~' && i + 1 < segment.Length)
			{
				if (segment[i + 1] == '0')
					result[writeIndex++] = '~';
				else if (segment[i + 1] == '1')
					result[writeIndex++] = '/';
				i++;
			}
			else
			{
				result[writeIndex++] = segment[i];
			}
		}

		return new string(result, 0, writeIndex);
	}

	/// <summary>
	/// Compares this pointerOld with another pointerOld for equality.
	/// </summary>
	public bool Equals(JsonPointer other)
	{
		if (_segmentCount != other._segmentCount)
			return false;

		return _pointer.Span.SequenceEqual(other._pointer.Span);
	}

	public override bool Equals(object? obj)
	{
		return obj is JsonPointer other && Equals(other);
	}

	public override int GetHashCode()
	{
		var span = _pointer.Span;
		int hash = 0;
		for (int i = 0; i < span.Length; i++)
		{
			hash = ((hash << 5) + hash) ^ span[i];
		}
		return hash;
	}

	public static bool operator ==(JsonPointer left, JsonPointer right) => left.Equals(right);
	public static bool operator !=(JsonPointer left, JsonPointer right) => !left.Equals(right);

	/// <summary>
	/// Compares a segment of this pointerOld with a value, handling both encoded and unencoded values.
	/// </summary>
	/// <param name="segmentIndex">The index of the segment to compare</param>
	/// <param name="value">The value to compare against (can be encoded or unencoded)</param>
	/// <returns>True if the segment matches the value, false otherwise</returns>
	public bool SegmentEquals(int segmentIndex, ReadOnlySpan<char> value)
	{
		if (segmentIndex < 0 || segmentIndex >= _segmentCount)
			throw new ArgumentOutOfRangeException(nameof(segmentIndex));

		var segment = GetSegment(segmentIndex).Span;
            
		// If neither contains escape sequences, we can do a direct comparison
		if (segment.IndexOf('~') == -1 && value.IndexOf('~') == -1)
		{
			return segment.SequenceEqual(value);
		}

		// Otherwise, compare while handling escape sequences
		int i = 0, j = 0;
		while (i < segment.Length && j < value.Length)
		{
			if (segment[i] == '~' && i + 1 < segment.Length)
			{
				if (segment[i + 1] == '0')
				{
					if (value[j] != '~') return false;
					i += 2;
					j++;
				}
				else if (segment[i + 1] == '1')
				{
					if (value[j] != '/') return false;
					i += 2;
					j++;
				}
				else
				{
					if (segment[i] != value[j]) return false;
					i++;
					j++;
				}
			}
			else if (value[j] == '~' && j + 1 < value.Length)
			{
				if (value[j + 1] == '0')
				{
					if (segment[i] != '~') return false;
					i++;
					j += 2;
				}
				else if (value[j + 1] == '1')
				{
					if (segment[i] != '/') return false;
					i++;
					j += 2;
				}
				else
				{
					if (segment[i] != value[j]) return false;
					i++;
					j++;
				}
			}
			else
			{
				if (segment[i] != value[j]) return false;
				i++;
				j++;
			}
		}

		return i == segment.Length && j == value.Length;
	}

	/// <summary>
	/// Compares a segment of this pointerOld with a value, handling both encoded and unencoded values.
	/// </summary>
	/// <param name="segmentIndex">The index of the segment to compare</param>
	/// <param name="value">The value to compare against (can be encoded or unencoded)</param>
	/// <returns>True if the segment matches the value, false otherwise</returns>
	public bool SegmentEquals(int segmentIndex, string value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));
		return SegmentEquals(segmentIndex, value.AsSpan());
	}

	/// <summary>
	/// Evaluates this pointerOld against a JsonElement to find the referenced value.
	/// </summary>
	/// <param name="element">The root JsonElement to evaluate against</param>
	/// <returns>The referenced JsonElement if found, null otherwise</returns>
	public JsonElement? Evaluate(JsonElement element)
	{
		if (_pointer.IsEmpty)
			return element;

		var current = element;
		var span = _pointer.Span;
		int start = 1; // Skip the leading '/'
		int currentIndex = 0;

		while (start < span.Length)
		{
			int end = span[start..].IndexOf('/');
			if (end == -1)
				end = span.Length;
			else
				end += start;

			var segment = span[start..end];

			if (current.ValueKind == JsonValueKind.Object)
			{
				bool found = false;
				foreach (var property in current.EnumerateObject())
				{
					if (SegmentEquals(currentIndex, property.Name))
					{
						current = property.Value;
						found = true;
						break;
					}
				}
				if (!found)
					return null;
			}
			else if (current.ValueKind == JsonValueKind.Array)
			{
				if (!segment.TryParse(out int index) || index < 0 || index >= current.GetArrayLength())
					return null;
				current = current[index];
			}
			else
			{
				return null;
			}

			start = end + 1;
			currentIndex++;
		}

		return current;
	}

	/// <summary>
	/// Evaluates the pointerOld over a <see cref="JsonNode"/>.
	/// </summary>
	/// <param name="root">The <see cref="JsonNode"/>.</param>
	/// <param name="result">The result, if return value is true; null otherwise</param>
	/// <returns>true if a value exists at the indicate path; false otherwise.</returns>
	public bool TryEvaluate(JsonNode? root, out JsonNode? result)
	{
		if (_pointer.IsEmpty)
		{
			result = root;
			return true;
		}

		var current = root;
		result = null;
		var span = _pointer.Span;
		int start = 1; // Skip the leading '/'
		int currentIndex = 0;

		while (start < span.Length)
		{
			int end = span[start..].IndexOf('/');
			if (end == -1)
				end = span.Length;
			else
				end += start;

			var segment = span[start..end];

			switch (current)
			{
				case JsonArray array:
					if (segment.Length == 0) return false;
					if (segment is ['0'])
					{
						if (array.Count == 0) return false;
						current = array[0];
						break;
					}
					if (segment[0] == '0') return false;
					if (segment is ['-'])
					{
						result = array.Last();
						return true;
					}
					if (!segment.TryParse(out int index)) return false;
					if (index >= array.Count) return false;
					if (index < 0) return false;
					current = array[index];
					break;
				case JsonObject obj:
					var found = false;
					foreach (var kvp in obj)
					{
						if (SegmentEquals(currentIndex, kvp.Key))
						{
							current = kvp.Value;
							found = true;
							break;
						}
					}
					if (!found) return false;
					break;
				default:
					return false;
			}

			start = end + 1;
			currentIndex++;
		}

		result = current;
		return true;
	}
}