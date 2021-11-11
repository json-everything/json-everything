using System;
using System.Text;

namespace Json.Pointer
{
	/// <summary>
	/// Represents a single segment of a JSON Pointer.
	/// </summary>
	public class PointerSegment : IEquatable<PointerSegment>
	{
		/// <summary>
		/// Gets the source string.
		/// </summary>
		public string Source { get; private set; }
		/// <summary>
		/// Gets the segment value.
		/// </summary>
		/// <remarks>
		/// This may differ from <see cref="Source"/> in that the segment may be URL-encoded.  This contains the decoded value.
		/// </remarks>
		public string Value { get; private set; }

#pragma warning disable CS8618
		private PointerSegment(){}
#pragma warning restore CS8618

		/// <summary>
		/// Parses a JSON Pointer segment from a string.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <param name="uriFormatted">Indicates whether the segment should be URL-decoded.</param>
		/// <returns>A JSON Pointer segment.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
		/// <exception cref="PointerParseException"><paramref name="source"/> contains an invalid escape sequence or an invalid URI-encoded sequence or ends with `~`.</exception>
		public static PointerSegment Parse(string? source, bool uriFormatted)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return new PointerSegment
				{
					Source = source,
					Value = Decode(source, uriFormatted)
				};
		}

		/// <summary>
		/// Parses a JSON Pointer segment from a string.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <param name="uriFormatted">Indicates whether the segment should be URL-decoded.</param>
		/// <param name="segment">The resulting segments.</param>
		/// <returns><code>true</code> if the parse was successful; <code>false</code> otherwise.</returns>
		public static bool TryParse(string? source, bool uriFormatted, out PointerSegment? segment)
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
						Source = source,
						Value = Decode(source, uriFormatted)
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
		/// <param name="uriFormatted">Whether the segment should be URL-encoded.</param>
		/// <returns></returns>
		public static PointerSegment Create(string value, bool uriFormatted = false)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));

			return new PointerSegment
				{
					Value = value,
					Source = Encode(value, uriFormatted)
				};
		}

		private static string Encode(string value, bool uriFormatted)
		{
			var builder = new StringBuilder();
			var chars = value.AsSpan();
			foreach (var ch in chars)
			{
				switch (ch)
				{
					case '~':
						builder.Append("~0");
						break;
					case '/':
						builder.Append("~1");
						break;
					case '%' when uriFormatted:
						builder.Append("%25");
						break;
					case '^' when uriFormatted:
						builder.Append("%5E");
						break;
					case '|' when uriFormatted:
						builder.Append("%7C");
						break;
					case '\\' when uriFormatted:
						builder.Append("%5C");
						break;
					case '\"' when uriFormatted:
						builder.Append("%22");
						break;
					case ' ' when uriFormatted:
						builder.Append("%20");
						break;
					default:
						builder.Append(ch);
						break;
				}
			}

			return builder.ToString();
		}

		private static string Decode(string source, bool uriFormatted)
		{
			var builder = new StringBuilder();
			var span = source.AsSpan();
			for (int i = 0; i < span.Length; i++)
			{
				if (span[i] == '~')
				{
					if (i >= span.Length - 1) throw new PointerParseException("Segment cannot end with `~`");

					switch (span[i + 1])
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
							throw new PointerParseException($"Invalid escape sequence: `~{span[i + 1]}`");
					}

					continue;
				}

				if (uriFormatted && span[i] == '%')
				{
					if (i <= span.Length - 3 && span[i + 1].IsHexadecimal() && span[i + 2].IsHexadecimal())
					{
						var ch = (char) ((span[i + 1].GetHexadecimalValue() << 4) + span[i + 2].GetHexadecimalValue());
						builder.Append(ch);
						i += 2;
						continue;
					}

					throw new PointerParseException("For URI-encoded pointers, `%` must be followed by two hexadecimal characters");
				}

				builder.Append(span[i]);
			}

			return builder.ToString();
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(PointerSegment? other)
		{
			return string.Equals(Value, other?.Value, StringComparison.InvariantCulture);
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is PointerSegment other && Equals(other);
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
		/// <returns><code>true</code> if the pointers are equal; <code>false</code> otherwise.</returns>
		public static bool operator ==(PointerSegment left, PointerSegment right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Evaluates inequality via <see cref="Equals(PointerSegment)"/>.
		/// </summary>
		/// <param name="left">A JSON Pointer.</param>
		/// <param name="right">A JSON Pointer.</param>
		/// <returns><code>false</code> if the pointers are equal; <code>true</code> otherwise.</returns>
		public static bool operator !=(PointerSegment left, PointerSegment right)
		{
			return !left.Equals(right);
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
}