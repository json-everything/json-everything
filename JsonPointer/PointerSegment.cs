using System;
using System.Linq;
using System.Text;
using System.Web;

namespace Json.Pointer;

/// <summary>
/// Represents a single segment of a JSON Pointer.
/// </summary>
public class PointerSegment : IEquatable<PointerSegment>
{
	private string? _source;

	/// <summary>
	/// Gets the segment value.
	/// </summary>
	public string Value { get; private set; }

#pragma warning disable CS8618
	private PointerSegment(){}
#pragma warning restore CS8618

	/// <summary>
	/// Parses a JSON Pointer segment from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <returns>A JSON Pointer segment.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	/// <exception cref="PointerParseException"><paramref name="source"/> contains an invalid escape sequence or an invalid URI-encoded sequence or ends with `~`.</exception>
	public static PointerSegment Parse(string? source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		return new PointerSegment
		{
			_source = source,
			Value = Decode(source)
		};
	}

	/// <summary>
	/// Parses a JSON Pointer segment from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="segment">The resulting segments.</param>
	/// <returns>`true` if the parse was successful; `false` otherwise.</returns>
	public static bool TryParse(string? source, out PointerSegment? segment)
	{
		if (source == null)
		{
			segment = null;
			return false;
		}

		try
		{
			segment = new PointerSegment
			{
				_source = source,
				Value = Decode(source)
			};
			return true;
		}
		catch (PointerParseException)
		{
			segment = default;
			return false;
		}
	}

	/// <summary>
	/// Creates a new <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns></returns>
	public static PointerSegment Create(string value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));

		return new PointerSegment
		{
			Value = value
		};
	}

	private static string Encode(string value)
	{
		if (value.All(c => c is not ('~' or '/'))) return value;

		var builder = new StringBuilder();
		foreach (var ch in value)
		{
			switch (ch)
			{
				case '~':
					builder.Append("~0");
					break;
				case '/':
					builder.Append("~1");
					break;
				default:
					builder.Append(ch);
					break;
			}
		}

		return builder.ToString();
	}

	private static string Decode(string source)
	{
		if (source.All(c => c is not '~')) return source;

		var builder = new StringBuilder();
		for (int i = 0; i < source.Length; i++)
		{
			if (source[i] == '~')
			{
				if (i >= source.Length - 1) throw new PointerParseException("Segment cannot end with `~`");

				switch (source[i + 1])
				{
					case '0':
						builder.Append('~');
						i++;
						break;
					case '1':
						builder.Append('/');
						i++;
						break;
					default:
						throw new PointerParseException($"Invalid escape sequence: `~{source[i + 1]}`");
				}

				continue;
			}

			builder.Append(source[i]);
		}

		return builder.ToString();
	}

	/// <summary>Returns the string representation of this instance.</summary>
	/// <param name="pointerStyle">Indicates whether to URL-encode the pointer.</param>
	/// <returns>The string representation.</returns>
	public string ToString(JsonPointerStyle pointerStyle)
	{
		_source ??= Encode(Value);
		var str = _source;

		if (pointerStyle == JsonPointerStyle.UriEncoded)
			str = HttpUtility.UrlEncode(str);

		return str;
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(PointerSegment? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;

		return string.Equals(Value, other.Value, StringComparison.InvariantCulture);
	}

	/// <summary>Indicates whether this instance and a specified object are equal.</summary>
	/// <param name="obj">The object to compare with the current instance.</param>
	/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
	public override bool Equals(object? obj)
	{
		return Equals(obj as PointerSegment);
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
	public override int GetHashCode()
	{
		// ReSharper disable once NonReadonlyMemberInGetHashCode
		return StringComparer.InvariantCulture.GetHashCode(Value);
	}

	/// <summary>
	/// Evaluates equality via <see cref="Equals(PointerSegment)"/>.
	/// </summary>
	/// <param name="left">A JSON Pointer.</param>
	/// <param name="right">A JSON Pointer.</param>
	/// <returns>`true` if the pointers are equal; `false` otherwise.</returns>
	public static bool operator ==(PointerSegment left, PointerSegment right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Evaluates inequality via <see cref="Equals(PointerSegment)"/>.
	/// </summary>
	/// <param name="left">A JSON Pointer.</param>
	/// <param name="right">A JSON Pointer.</param>
	/// <returns>`false` if the pointers are equal; `true` otherwise.</returns>
	public static bool operator !=(PointerSegment left, PointerSegment right)
	{
		return !left.Equals(right);
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
		return Create(value.ToString());
	}

	/// <summary>
	/// Implicitly casts an <see cref="uint"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	/// <remarks>No URI encoding is performed for implicit casts.</remarks>
	public static implicit operator PointerSegment(uint value)
	{
		return Create(value.ToString());
	}

	/// <summary>
	/// Implicitly casts a <see cref="string"/> to a <see cref="PointerSegment"/>.
	/// </summary>
	/// <param name="value">A pointer segment that represents the value.</param>
	/// <remarks>No URI encoding is performed for implicit casts.</remarks>
	public static implicit operator PointerSegment(string value)
	{
		return Create(value);
	}
}