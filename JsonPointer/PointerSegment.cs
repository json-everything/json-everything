using System;

namespace Json.Pointer;

/// <summary>
/// Serves as an intermediary for creating JSON Pointers by segments.
/// </summary>
public readonly struct PointerSegment
{
	internal string Value { get; }

	private PointerSegment(string value)
	{
		Value = value;
	}

	/// <summary>
	/// Implicitly casts an <see cref="uint"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	public static implicit operator PointerSegment(int value) =>
		new(value.ToString());

	/// <summary>
	/// Implicitly casts a <see cref="string"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	/// <remarks>JSON Pointer encoding is performed, but URI encoding is not.</remarks>
	public static implicit operator PointerSegment(string value) =>
		new(value);

	/// <summary>Returns the fully qualified type name of this instance.</summary>
	/// <returns>The fully qualified type name.</returns>
	public override string ToString() => Value;
}