using System;
using System.Text;

namespace Json.Path;

internal class NameSelector : ISelector
{
	public string Name { get; set; }
}

internal class NameSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ISelector? selector)
	{
		char quoteChar;
		var i = index;
		switch (source[index])
		{
			case '"':
				quoteChar = '"';
				i++;
				break;
			case '\'':
				quoteChar = '\'';
				i++;
				break;
			default:
				selector = null;
				return false;
		}

		var done = false;
		var escaped = false;
		var sb = new StringBuilder();
		while (i < source.Length && !done)
		{
			if (escaped)
			{
				if (!ReadEscaped(source, quoteChar, sb, ref i))
				{
					selector = null;
					return false;
				}

				escaped = false;
			}
			else if (source[i] == '\\')
			{
				escaped = true;
				i++;
			}
			else if (source[i] == quoteChar)
			{
				done = true;
				i++;
			}
			else
			{
				sb.Append(source[i]);
				i++;
			}
		}

		if (!done)
		{
			selector = null;
			return false;
		}

		index = i;
		selector = new NameSelector
		{
			Name = sb.ToString()
		};
		return true;
	}

	private static bool ReadEscaped(ReadOnlySpan<char> source, char quoteChar, StringBuilder sb, ref int i)
	{
		switch (source[i])
		{
			case 'b':
				sb.Append('\b');
				i++;
				break;
			case 't':
				sb.Append('\t');
				i++;
				break;
			case 'n':
				sb.Append('\n');
				i++;
				break;
			case 'f':
				sb.Append('\f');
				i++;
				break;
			case 'r':
				sb.Append('\r');
				i++;
				break;
			case '\\':
				sb.Append('\\');
				i++;
				break;
			default:
				if (source[i] == quoteChar)
				{
					sb.Append(quoteChar);
					i++;
					break;
				}
				// TODO add error messages
				return false;
		}

		return true;
	}
}