using System;

namespace Json.Pointer;

/// <summary>
/// Represents a segment of a JSON Pointer.
/// This is a ref struct to ensure zero allocations.
/// </summary>
// ReSharper disable once StructLacksIEquatable.Global
public readonly ref struct JsonPointerSegment
{
    private readonly ReadOnlySpan<char> _segment;

    internal JsonPointerSegment(ReadOnlySpan<char> segment)
    {
        _segment = segment;
    }

    private bool SegmentEquals(ReadOnlySpan<char> value)
    {
        // If neither contains escape sequences, we can do a direct comparison
        if (_segment.IndexOf('~') == -1 && value.IndexOf('~') == -1)
            return _segment.SequenceEqual(value);

        // Otherwise, compare while handling escape sequences
        int i = 0, j = 0;
        while (i < _segment.Length && j < value.Length)
        {
            if (_segment[i] == '~' && i + 1 < _segment.Length)
            {
                if (_segment[i + 1] == '0')
                {
                    if (value[j] != '~') return false;
                    i += 2;
                    j++;
                }
                else if (_segment[i + 1] == '1')
                {
                    if (value[j] != '/') return false;
                    i += 2;
                    j++;
                }
                else
                {
                    if (_segment[i] != value[j]) return false;
                    i++;
                    j++;
                }
            }
            else if (value[j] == '~' && j + 1 < value.Length)
            {
                if (value[j + 1] == '0')
                {
                    if (_segment[i] != '~') return false;
                    i++;
                    j += 2;
                }
                else if (value[j + 1] == '1')
                {
                    if (_segment[i] != '/') return false;
                    i++;
                    j += 2;
                }
                else
                {
                    if (_segment[i] != value[j]) return false;
                    i++;
                    j++;
                }
            }
            else
            {
                if (_segment[i] != value[j]) return false;
                i++;
                j++;
            }
        }

        return i == _segment.Length && j == value.Length;
    }

    /// <summary>
    /// Compares this segment with a string, handling both encoded and unencoded values.
    /// </summary>
    /// <param name="value">The value to compare against (can be encoded or unencoded)</param>
    /// <returns>True if the segment matches the value, false otherwise</returns>
    public bool Equals(string? value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        return SegmentEquals(value.AsSpan());
    }

    /// <summary>
    /// Compares this segment with an integer.
    /// </summary>
    /// <param name="value">The value to compare against</param>
    /// <returns>True if the segment matches the value, false otherwise</returns>
    public bool Equals(int value)
    {
        return SegmentEquals(value.ToString().AsSpan());
    }

    /// <summary>
    /// Compares this segment with another segment.
    /// </summary>
    /// <param name="other">The segment to compare against.</param>
    /// <returns>True if the segments are equal; false otherwise.</returns>
    public bool Equals(JsonPointerSegment other) => 
        SegmentEquals(other._segment);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
	    if (obj is string str) return Equals(str);

	    return false;
    }

    /// <summary>
    /// Compares two segments for equality.
    /// </summary>
    public static bool operator ==(JsonPointerSegment left, JsonPointerSegment right) => 
        left.Equals(right);

    /// <summary>
    /// Compares two segments for inequality.
    /// </summary>
    public static bool operator !=(JsonPointerSegment left, JsonPointerSegment right) => 
        !left.Equals(right);

    /// <summary>
    /// Compares a segment with a string for equality.
    /// </summary>
    public static bool operator ==(JsonPointerSegment left, string right) => 
        left.SegmentEquals(right.AsSpan());

    /// <summary>
    /// Compares a segment with a string for inequality.
    /// </summary>
    public static bool operator !=(JsonPointerSegment left, string right) => 
        !left.SegmentEquals(right.AsSpan());

    /// <summary>
    /// Compares a segment with a span for equality.
    /// </summary>
    public static bool operator ==(JsonPointerSegment left, ReadOnlySpan<char> right) => 
        left.SegmentEquals(right);

    /// <summary>
    /// Compares a segment with a span for inequality.
    /// </summary>
    public static bool operator !=(JsonPointerSegment left, ReadOnlySpan<char> right) => 
        !left.SegmentEquals(right);

    /// <summary>(deprecated) Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    [Obsolete]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    public override int GetHashCode() => _segment.GetHashCode();
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    /// <summary>
    /// Returns the string representation of this segment.
    /// </summary>
    /// <returns>The segment as a string.</returns>
    public override string ToString() => _segment.ToString();

    /// <summary>
    /// Returns the int representation of this segment.
    /// </summary>
    /// <returns>The segment as an int, or null if the segment is not a valid integer.</returns>
    public int? ToInt()
    {
#if NET8_0_OR_GREATER
		if (int.TryParse(_segment, out var result)) return result;
        
        return null;
#else
		if (_segment.Length == 0)
			return null;

		int result = 0;
		for (int i = 0; i < _segment.Length; i++)
		{
			char c = _segment[i];
			if (c < '0' || c > '9')
				return null;
			
			if (result > (int.MaxValue - (c - '0')) / 10)
				return null;
			
			result = result * 10 + (c - '0');
		}
		
		return result;
#endif
	}

    /// <summary>
    /// Gets the raw span representation of this segment.
    /// </summary>
    /// <returns>The segment as a ReadOnlySpan&lt;char&gt;.</returns>
    public ReadOnlySpan<char> AsSpan() => 
        _segment;
} 