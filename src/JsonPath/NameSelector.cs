using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Represents a name selector.
/// </summary>
public class NameSelector : ISelector, IHaveShorthand
{
	/// <summary>
	/// Gets the name.
	/// </summary>
	public string Name { get; }

	internal NameSelector(string name)
	{
		Name = name;
	}

	string IHaveShorthand.ToShorthandString()
	{
		return $".{Name}";
	}

	void IHaveShorthand.AppendShorthandString(StringBuilder builder)
	{
		builder.Append('.');
		builder.Append(Name);
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return $"'{Name}'"; // TODO escape this
	}

	/// <summary>
	/// Evaluates the selector.
	/// </summary>
	/// <param name="match">The node to evaluate.</param>
	/// <param name="rootNode">The root node (typically used by filter selectors, e.g. `$[?@foo &lt; $.bar]`)</param>
	/// <returns>
	/// A collection of nodes.
	///
	/// Semantically, this is a nodelist, but leaving as IEnumerable&lt;Node&gt; allows for deferred execution.
	/// </returns>
	public IEnumerable<Node> Evaluate(Node match, JsonNode? rootNode)
	{
		var node = match.Value;
		if (node is not JsonObject obj) yield break;

		if (obj.TryGetPropertyValue(Name, out var value)) yield return new Node(value, match.Location!.Append(Name));
	}

	/// <summary>
	/// Builds a string using a string builder.
	/// </summary>
	/// <param name="builder">The string builder.</param>
	public void BuildString(StringBuilder builder)
	{
		builder.Append('\'');
		foreach (var c in Name)
		{
			builder.Append(c switch
			{
				'\'' => "\\'",
				'\b' => "\\b",
				'\t' => "\\t",
				'\n' => "\\n",
				'\f' => "\\f",
				'\r' => "\\r",
				'\\' => @"\\",
				_ => c
			});
		}
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
				var hexStart = i;
				while (ReadHexCode(source, ref i) && source[i] == '\\')
				{
					i++; // consume \
				}

				if (hexStart == i) return false;

				// this is simpler than trying to parse and calc surrogates myself.
				// but it does throw an InvalidOperationException when the encoded char is invalid...
				try
				{
					var hexEncodedChars = JsonNode.Parse($"\"{source[(hexStart-1)..i].ToString()}\"")!;
					sb.Append(hexEncodedChars.GetValue<string>());
				}
				catch (Exception e) when (e  is InvalidOperationException or JsonException)
				{
					return false;
				}
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
		if (source[i] != 'u') return false;

		var j = i;
		j++; // consume u
		if (j + 4 <= source.Length && source[j..(j + 4)].ToArray().All(x => char.ToUpper(x) is (>= 'A' and <= 'F') or (>= '0' and <= '9')))
		{
			i = j + 4;
			return true;
		}

		return false;
	}
}