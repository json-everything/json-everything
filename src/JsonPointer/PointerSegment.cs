using System;
using System.Globalization;

namespace Json.Pointer;

/// <summary>
/// Serves as an intermediary for creating JSON Pointers by segments.
/// </summary>
public readonly struct PointerSegment
{
	internal string Value { get; }

	private PointerSegment(string value)
	{
		Value = Encode(value);
	}

	private static string Encode(string value)
	{
		if (string.IsNullOrEmpty(value)) return string.Empty;

		// Calculate the required length
		int length = 0;
		foreach (char c in value)
		{
			length += c switch
			{
				'~' => 2, // ~0
				'/' => 2, // ~1
				_ => 1
			};
		}

		// If no encoding needed, return original
		if (length == value.Length) return value;

		// Create encoded string
		Span<char> encoded = stackalloc char[length];
		int pos = 0;
		foreach (char c in value)
		{
			switch (c)
			{
				case '~':
					encoded[pos++] = '~';
					encoded[pos++] = '0';
					break;
				case '/':
					encoded[pos++] = '~';
					encoded[pos++] = '1';
					break;
				default:
					encoded[pos++] = c;
					break;
			}
		}

		return encoded.ToString();
	}

	/// <summary>
	/// Implicitly casts an <see cref="int"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	public static implicit operator PointerSegment(int value) =>
		new(value.ToString(CultureInfo.InvariantCulture));

	/// <summary>
	/// Implicitly casts a <see cref="string"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	/// <remarks>JSON Pointer encoding is performed automatically.</remarks>
	public static implicit operator PointerSegment(string value) =>
		new(value);

	/// <summary>Returns the fully qualified type name of this instance.</summary>
	/// <returns>The fully qualified type name.</returns>
	public override string ToString() => Value;
}