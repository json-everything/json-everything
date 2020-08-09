using System;
using Cysharp.Text;

namespace Json.Pointer
{
	public struct PointerSegment
	{
		public string Source { get; private set; }
		public string Value { get; private set; }

		public static PointerSegment Parse(string source, bool uriFormatted)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			return new PointerSegment
				{
					Source = source,
					Value = _Decode(source, uriFormatted)
				};
		}

		public static bool TryParse(string source, bool uriFormatted, out PointerSegment segment)
		{
			if (source == null)
			{
				segment = default;
				return false;
			}

			try
			{
				segment = new PointerSegment
					{
						Source = source,
						Value = _Decode(source, uriFormatted)
					};
			}
			catch (PointerParseException)
			{
				segment = default;
				return false;
			}
			return true;
		}

		public static PointerSegment Create(string value, bool uriFormatted = false)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));

			return new PointerSegment
				{
					Value = value,
					Source = _Encode(value, uriFormatted)
				};
		}

		private static string _Encode(string value, bool uriFormatted)
		{
			var builder = ZString.CreateStringBuilder();
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

		private static string _Decode(string source, bool uriFormatted)
		{
			var builder = ZString.CreateStringBuilder();
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
	}
}