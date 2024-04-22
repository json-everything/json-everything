using System;

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
	/// <remarks>No URI encoding is performed for implicit casts.</remarks>
	public static implicit operator PointerSegment(int value)
	{
		if (value < -1)
			throw new ArgumentOutOfRangeException(nameof(value));
		return new PointerSegment { Value = value.ToString() };
	}

	/// <summary>
	/// Implicitly casts an <see cref="uint"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	/// <remarks>No URI encoding is performed for implicit casts.</remarks>
	public static implicit operator PointerSegment(uint value)
	{
		return new PointerSegment { Value = value.ToString() };
	}

	/// <summary>
	/// Implicitly casts a <see cref="string"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	/// <remarks>No URI encoding is performed for implicit casts.</remarks>
	public static implicit operator PointerSegment(string value)
	{
		return new PointerSegment { Value = value };
	}

	/// <summary>Returns the fully qualified type name of this instance.</summary>
	/// <returns>The fully qualified type name.</returns>
	public override string ToString() => Value;
}