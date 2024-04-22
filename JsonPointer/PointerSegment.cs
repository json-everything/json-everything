using System;
using System.Buffers;

namespace Json.Pointer;

/// <summary>
/// Serves as an intermediary for creating JSON Pointers by segments.
/// </summary>
public struct PointerSegment
{
	/// <summary>
	/// Gets the segment value.
	/// </summary>
	public string Value { get; private set; }

	/// <summary>
	/// Creates a new segment.
	/// </summary>
	public PointerSegment()
	{
		Value = string.Empty;
	}

	/// <summary>
	/// Implicitly casts an <see cref="uint"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	public static implicit operator PointerSegment(int value)
	{
		if (value < 0)
			throw new ArgumentOutOfRangeException(nameof(value));
		return new PointerSegment { Value = value.ToString() };
	}

	/// <summary>
	/// Implicitly casts a <see cref="string"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	/// <remarks>JSON Pointer encoding is performed, but URI encoding is not.</remarks>
	public static implicit operator PointerSegment(string value)
	{
		return new PointerSegment { Value = Encode(value) };
	}

	/// <summary>Returns the fully qualified type name of this instance.</summary>
	/// <returns>The fully qualified type name.</returns>
	public readonly override string ToString() => Value;

	private static string Encode(string key)
	{
		var owner = MemoryPool<char>.Shared.Rent();
		var span = owner.Memory.Span;

		var length = 0;
		foreach (var ch in key)
		{
			switch (ch)
			{
				case '~':
					span[length] = '~';
					span[length + 1] = '0';
					length+=2;
					break;
				case '/':
					span[length] = '~';
					span[length + 1] = '1';
					length+=2;
					break;
				default:
					span[length] = ch;
					length++;
					break;
			}
		}

		return span[..length].ToString();
	}
}