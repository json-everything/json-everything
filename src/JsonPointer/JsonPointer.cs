using System;
using System.Buffers;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Net;

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

	/// <summary>
	/// Gets the number of segments in the pointer.
	/// </summary>
	public int SegmentCount { get; }

	private JsonPointer(ReadOnlyMemory<char> pointer, int segmentCount)
	{
		_pointer = pointer;
		SegmentCount = segmentCount;
	}

	/// <summary>
	/// Gets the parent pointer of this pointer.
	/// </summary>
	/// <param name="levels">The number of ancestor levels to go back. Defaults to 1.</param>
	/// <returns>The parent pointer, or null if this is the root pointer</returns>
	public JsonPointer? GetParent(int levels = 1)
	{
		if (levels <= 0) throw new ArgumentOutOfRangeException(nameof(levels), "Levels must be positive");
		if (SegmentCount < levels) return null;
		if (SegmentCount == levels) return Empty;

		var span = _pointer.Span;
		int lastSlash = span.LastIndexOf('/');
		if (lastSlash <= 0) return null;

		// Find the nth slash from the end
		for (int i = 0; i < levels - 1; i++)
		{
			lastSlash = span[..lastSlash].LastIndexOf('/');
			if (lastSlash <= 0) return null;
		}

		return new JsonPointer(_pointer[..lastSlash], SegmentCount - levels);
	}

	/// <summary>
	/// Gets the local pointer (trailing end) of this pointer.
	/// </summary>
	/// <param name="levels">The number of segments to keep from the end. Defaults to 1.</param>
	/// <returns>A new pointer containing the specified number of trailing segments</returns>
	public JsonPointer GetLocal(int levels = 1)
	{
		if (levels <= 0) throw new ArgumentOutOfRangeException(nameof(levels), "Levels must be positive");
		if (levels > SegmentCount) throw new ArgumentOutOfRangeException(nameof(levels), "Levels cannot exceed segment count");

		var span = _pointer.Span;
		int start = 0;

		// Find the start of the nth segment from the end
		for (int i = 0; i < SegmentCount - levels; i++)
		{
			start = span[start..].IndexOf('/') + start + 1;
		}

		return new JsonPointer(_pointer[start..], levels);
	}

	/// <summary>
	/// Creates a new JSON Pointer from segments.
	/// </summary>
	/// <param name="segments">The segments to combine.</param>
	/// <returns>A new JSON Pointer.</returns>
	/// <remarks>
	/// This method incurs allocation costs for string concatenation and array creation.
	/// For better performance with large pointers, consider using <see cref="Parse(ReadOnlySpan{char})"/>.
	/// </remarks>
	public static JsonPointer Create(params PointerSegment[] segments)
	{
		if (segments.Length == 0) return Empty;

		var totalLength = 0;
		foreach (var segment in segments)
		{
			totalLength += segment.Value.Length + 1; // +1 for the '/'
		}

		// Use stackalloc for small pointers (up to 256 chars)
		if (totalLength <= 256)
		{
			Span<char> combined = stackalloc char[totalLength];
			var currentPos = 0;

			foreach (var segment in segments)
			{
				combined[currentPos++] = '/';
				segment.Value.AsSpan().CopyTo(combined[currentPos..]);
				currentPos += segment.Value.Length;
			}

			return new JsonPointer(combined.ToArray(), segments.Length);
		}

		// Use ArrayPool for larger pointers
		char[]? rented = null;
		try
		{
			rented = ArrayPool<char>.Shared.Rent(totalLength);
			var combined = rented.AsSpan(0, totalLength);
			var currentPos = 0;

			foreach (var segment in segments)
			{
				combined[currentPos++] = '/';
				segment.Value.AsSpan().CopyTo(combined[currentPos..]);
				currentPos += segment.Value.Length;
			}

			return new JsonPointer(combined.ToArray(), segments.Length);
		}
		finally
		{
			if (rented != null) 
				ArrayPool<char>.Shared.Return(rented);
		}
	}

	/// <summary>
	/// Combines this pointer with another pointer.
	/// </summary>
	/// <param name="other">The pointer to append</param>
	/// <returns>A new pointer representing the combination</returns>
	public JsonPointer Combine(JsonPointer other)
	{
		if (other._pointer.IsEmpty) return this;

		if (_pointer.IsEmpty) return other;

		int totalLength = _pointer.Length + other._pointer.Length;
            
		// Use stackalloc for small pointers (up to 256 chars)
		if (totalLength <= 256)
		{
			Span<char> combined = stackalloc char[totalLength];
			_pointer.Span.CopyTo(combined);
			other._pointer.Span.CopyTo(combined[_pointer.Length..]);
			return new JsonPointer(combined.ToArray(), SegmentCount + other.SegmentCount);
		}
            
		// Use ArrayPool for larger pointers
		char[]? rented = null;
		try
		{
			rented = ArrayPool<char>.Shared.Rent(totalLength);
			var combined = rented.AsSpan(0, totalLength);
			_pointer.Span.CopyTo(combined);
			other._pointer.Span.CopyTo(combined[_pointer.Length..]);
			return new JsonPointer(combined.ToArray(), SegmentCount + other.SegmentCount);
		}
		finally
		{
			if (rented != null)
				ArrayPool<char>.Shared.Return(rented);
		}
	}

	/// <summary>
	/// Combines this pointer with additional segments.
	/// </summary>
	/// <param name="segments">The segments to append</param>
	/// <returns>A new pointer representing the combination</returns>
	/// <remarks>
	/// This method incurs allocation costs for string concatenation and array creation.
	/// For better performance with large numbers of segments, consider using <see cref="Parse(ReadOnlySpan{char})"/>.
	/// </remarks>
	public JsonPointer Combine(params PointerSegment[] segments)
	{
		if (segments.Length == 0) return this;

		var totalLength = _pointer.Length;
		var totalSegments = SegmentCount;

		// Calculate total length and validate segments
		foreach (var segment in segments)
		{
			totalLength += segment.Value.Length + 1; // +1 for the '/'
			totalSegments++;
		}

		// Use stackalloc for small pointers (up to 256 chars)
		if (totalLength <= 256)
		{
			Span<char> combined = stackalloc char[totalLength];
			_pointer.Span.CopyTo(combined);
			var currentPos = _pointer.Length;

			foreach (var segment in segments)
			{
				combined[currentPos++] = '/';
				segment.Value.AsSpan().CopyTo(combined[currentPos..]);
				currentPos += segment.Value.Length;
			}

			return new JsonPointer(combined.ToArray(), totalSegments);
		}

		// Use ArrayPool for larger pointers
		char[]? rented = null;
		try
		{
			rented = ArrayPool<char>.Shared.Rent(totalLength);
			var combined = rented.AsSpan(0, totalLength);
			_pointer.Span.CopyTo(combined);
			var currentPos = _pointer.Length;

			foreach (var segment in segments)
			{
				combined[currentPos++] = '/';
				segment.Value.AsSpan().CopyTo(combined[currentPos..]);
				currentPos += segment.Value.Length;
			}

			return new JsonPointer(combined.ToArray(), totalSegments);
		}
		finally
		{
			if (rented != null) 
				ArrayPool<char>.Shared.Return(rented);
		}
	}

	/// <summary>
	/// Gets the string representation of this pointer.
	/// </summary>
	/// <returns>The pointer string</returns>
	public override string ToString() => _pointer.ToString();

	/// <summary>
	/// Gets a segment from the pointer by index.
	/// </summary>
	/// <param name="index">The zero-based index of the segment</param>
	/// <returns>The segment as a ReadOnlyMemory&lt;char&gt;</returns>
	public ReadOnlyMemory<char> GetSegment(int index)
	{
		if (index < 0 || index >= SegmentCount)
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
	/// Compares this pointer with another pointer for equality.
	/// </summary>
	public bool Equals(JsonPointer other)
	{
		if (SegmentCount != other.SegmentCount) return false;

		return _pointer.Span.SequenceEqual(other._pointer.Span);
	}

	/// <summary>Indicates whether this instance and a specified object are equal.</summary>
	/// <param name="obj">The object to compare with the current instance.</param>
	/// <returns>
	/// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
	public override bool Equals(object? obj)
	{
		return obj is JsonPointer other && Equals(other);
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
	public override int GetHashCode()
	{
		var span = _pointer.Span;
		int hash = 0;
		foreach (var c in span)
		{
			hash = ((hash << 5) + hash) ^ c;
		}
		return hash;
	}

	public static bool operator ==(JsonPointer left, JsonPointer right) => left.Equals(right);
	public static bool operator !=(JsonPointer left, JsonPointer right) => !left.Equals(right);

	/// <summary>
	/// Compares a segment of this pointer with a value, handling both encoded and unencoded values.
	/// </summary>
	/// <param name="segmentIndex">The index of the segment to compare</param>
	/// <param name="value">The value to compare against (can be encoded or unencoded)</param>
	/// <returns>True if the segment matches the value, false otherwise</returns>
	public bool SegmentEquals(int segmentIndex, ReadOnlySpan<char> value)
	{
		if (segmentIndex < 0 || segmentIndex >= SegmentCount)
			throw new ArgumentOutOfRangeException(nameof(segmentIndex));

		var segment = GetSegment(segmentIndex).Span;
            
		// If neither contains escape sequences, we can do a direct comparison
		if (segment.IndexOf('~') == -1 && value.IndexOf('~') == -1)
			return segment.SequenceEqual(value);

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
	/// Compares a segment of this pointer with a value, handling both encoded and unencoded values.
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
	/// Evaluates this pointer against a JsonElement to find the referenced value.
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
				if (!found) return null;
			}
			else if (current.ValueKind == JsonValueKind.Array)
			{
				if (!segment.TryParse(out int index) || index < 0 || index >= current.GetArrayLength())
					return null;
				current = current[index];
			}
			else return null;

			start = end + 1;
			currentIndex++;
		}

		return current;
	}

	/// <summary>
	/// Evaluates the pointer over a <see cref="JsonNode"/>.
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

		while (start <= span.Length)
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

	/// <summary>
	/// Creates a new JSON Pointer from a string.
	/// </summary>
	/// <param name="pointer">The string representation of the pointer.</param>
	/// <returns>A new JSON Pointer.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="pointer"/> is null.</exception>
	/// <exception cref="PointerParseException"><paramref name="pointer"/> is not a valid JSON Pointer.</exception>
	public static JsonPointer Parse(string pointer)
	{
		if (pointer == null)
			throw new ArgumentNullException(nameof(pointer));

		if (pointer.Length == 0) return Empty;

		// Handle URL-encoded pointers
		if (pointer[0] == '#')
		{
			if (pointer.Length == 1) return Empty;

			pointer = WebUtility.UrlDecode(pointer[1..]);
		}

		var (isValid, segmentCount) = ValidateAndCountSegments(pointer.AsSpan());
		if (!isValid)
			throw new PointerParseException("Invalid JSON Pointer format");

		return new JsonPointer(pointer.AsMemory(), segmentCount);
	}

	/// <summary>
	/// Attempts to create a new JSON Pointer from a string.
	/// </summary>
	/// <param name="pointer">The string representation of the pointer.</param>
	/// <param name="result">The resulting pointer.</param>
	/// <returns><c>true</c> if the pointer was successfully created; <c>false</c> otherwise.</returns>
	public static bool TryParse(string? pointer, out JsonPointer result)
	{
		result = default;

		if (string.IsNullOrEmpty(pointer))
		{
			result = Empty;
			return true;
		}

		// Handle URL-encoded pointers
		if (pointer[0] == '#')
		{
			if (pointer.Length == 1)
			{
				result = Empty;
				return true;
			}

			try
			{
				pointer = WebUtility.UrlDecode(pointer[1..]);
			}
			catch
			{
				return false;
			}
		}

		var (isValid, segmentCount) = ValidateAndCountSegments(pointer.AsSpan());
		if (!isValid)
			return false;

		result = new JsonPointer(pointer.AsMemory(), segmentCount);
		return true;
	}

	/// <summary>
	/// Creates a new JSON Pointer from a span.
	/// </summary>
	/// <param name="pointer">The span representation of the pointer.</param>
	/// <returns>A new JSON Pointer.</returns>
	/// <exception cref="PointerParseException"><paramref name="pointer"/> is not a valid JSON Pointer.</exception>
	public static JsonPointer Parse(ReadOnlySpan<char> pointer)
	{
		if (pointer.Length == 0) return Empty;

		// Handle URL-encoded pointers
		if (pointer[0] == '#')
		{
			if (pointer.Length == 1) return Empty;

			// Convert to string for URL decoding since WebUtility doesn't support spans
			var decoded = WebUtility.UrlDecode(pointer[1..].ToString());
			pointer = decoded.AsSpan();
		}

		var (isValid, segmentCount) = ValidateAndCountSegments(pointer);
		if (!isValid)
			throw new PointerParseException("Invalid JSON Pointer format");

		return new JsonPointer(pointer.ToArray(), segmentCount);
	}

	/// <summary>
	/// Attempts to create a new JSON Pointer from a span.
	/// </summary>
	/// <param name="pointer">The span representation of the pointer.</param>
	/// <param name="result">The resulting pointer.</param>
	/// <returns><c>true</c> if the pointer was successfully created; <c>false</c> otherwise.</returns>
	public static bool TryParse(ReadOnlySpan<char> pointer, out JsonPointer result)
	{
		result = default;

		if (pointer.Length == 0)
		{
			result = Empty;
			return true;
		}

		// Handle URL-encoded pointers
		if (pointer[0] == '#')
		{
			if (pointer.Length == 1)
			{
				result = Empty;
				return true;
			}

			// Convert to string for URL decoding since WebUtility doesn't support spans
			var decoded = WebUtility.UrlDecode(pointer[1..].ToString());
			pointer = decoded.AsSpan();
		}

		var (isValid, segmentCount) = ValidateAndCountSegments(pointer);
		if (!isValid)
			return false;

		result = new JsonPointer(pointer.ToArray(), segmentCount);
		return true;
	}

	private static (bool IsValid, int SegmentCount) ValidateAndCountSegments(ReadOnlySpan<char> pointer)
	{
		// Must start with '/'
		if (pointer[0] != '/') return (false, 0);

		int count = 0;
		for (int i = 0; i < pointer.Length; i++)
		{
			if (pointer[i] == '/')
			{
				count++;
			}
			else if (pointer[i] == '~')
			{
				// Must have a character after '~'
				if (i + 1 >= pointer.Length) return (false, 0);

				// Must be followed by '0' or '1'
				if (pointer[i + 1] != '0' && pointer[i + 1] != '1') return (false, 0);

				// Skip the next character since we've validated it
				i++;
			}
		}

		return (true, count);
	}
}