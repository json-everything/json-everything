using System;
using System.Buffers;

namespace Json.Pointer;

/// <summary>
/// Serves as an intermediary for creating JSON Pointers by segments.
/// </summary>
public struct PointerSegment
{
	private string? _stringValue;
	private int? _intValue;

	internal readonly ReadOnlySpan<char> GetValue()
	{
		if (_intValue.HasValue)
		{
			var owner = MemoryPool<char>.Shared.Rent(15);
			var span = owner.Memory.Span;
			var current = _intValue.Value;
			var length = GetLength();
			for (int i = 0; i < length; i++)
			{
				var digit = current % 10;
				span[^(i+1)] = (char)(digit + '0');
				current /= 10;
			}

			return span[^length..];
		}

		return _stringValue!.AsSpan();
	}

	internal readonly int GetLength() =>
		_intValue.HasValue
			? _intValue.Value / 10 + 1
			: _stringValue!.Length;

	/// <summary>
	/// Implicitly casts an <see cref="uint"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	public static implicit operator PointerSegment(int value) =>
		value >= 0
			? new PointerSegment { _intValue = value }
			: throw new ArgumentOutOfRangeException(nameof(value));

	/// <summary>
	/// Implicitly casts a <see cref="string"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	/// <remarks>JSON Pointer encoding is performed, but URI encoding is not.</remarks>
	public static implicit operator PointerSegment(string value) => 
		new() { _stringValue = value };

	/// <summary>Returns the fully qualified type name of this instance.</summary>
	/// <returns>The fully qualified type name.</returns>
	public readonly override string ToString() => _intValue?.ToString() ?? _stringValue!;
}