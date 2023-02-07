using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

internal class NameSelector : ISelector, IHaveShorthand
{
	public string Name { get; }

	public NameSelector(string name)
	{
		Name = name;
	}

	public string ToShorthandString()
	{
		return $".{Name}";
	}

	public void AppendShorthandString(StringBuilder builder)
	{
		builder.Append('.');
		builder.Append(Name);
	}

	public override string ToString()
	{
		return $"'{Name}'"; // TODO escape this
	}

	public IEnumerable<Node> Evaluate(Node match, JsonNode? rootNode)
	{
		var node = match.Value;
		if (node is not JsonObject obj) yield break;

		if (obj.TryGetPropertyValue(Name, out var value)) yield return new Node(value, match.Location.Append(Name));
	}

	public void BuildString(StringBuilder builder)
	{
		builder.Append('\'');
		builder.Append(Name); // TODO escape this
		builder.Append('\'');
	}
}

internal class NameSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ISelector? selector, PathParsingOptions options)
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
				if (!source.EnsureValidNameCharacter(i))
				{
					selector = null;
					return false;
				}
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
		selector = new NameSelector(sb.ToString());
		return true;
	}

	private static bool ReadEscaped(ReadOnlySpan<char> source, char quoteChar, StringBuilder sb, ref int i)
	{
		// TODO handle multi-char escapes
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
			case '/':
				sb.Append('/');
				i++;
				break;
			case 'u':
			case 'U':
				var hexStart = i;
				while (ReadHexCode(source, ref i) && source[i] == '\\')
				{
					i++; // consume \
				}

				if (hexStart == i) return false;

				// this is simpler than trying to parse and calc surrogates myself.
				var hexEncodedChars = JsonNode.Parse($"\"{source[(hexStart-1)..i].ToString()}\"")!;
				sb.Append(hexEncodedChars.GetValue<string>());
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

	private static bool ReadHexCode(ReadOnlySpan<char> source, ref int i)
	{
		// reads uXXXX
		if (source[i] is not 'u' or 'U') return false;

		i++; // consume u
		if (source[i..(i + 4)].ToArray().All(x => char.ToUpper(x) is (>= 'A' and <= 'F') or (>= '0' and <= '9')))
		{
			i += 4;
			return true;
		}

		return false;
	}
}