using System;
using System.Collections.Generic;

namespace Json.Path;

internal static class PathParser
{
	private static readonly ISelectorParser[] _parsers =
	{
		new FilterSelectorParser(),
		new SliceSelectorParser(),
		new IndexSelectorParser(),
		new NameSelectorParser(),
		new WildcardSelectorParser()
	};

	public static JsonPath Parse(ReadOnlySpan<char> source, bool requireGlobal = false)
	{
		if (source.Length == 0)
			throw new PathParseException(0, "Input string is empty");

		var index = 0;
		var segments = new List<PathSegment>();
		PathScope scope;

		if (source[0] == '$')
			scope = PathScope.Global;
		else if (requireGlobal)
			throw new PathParseException(index, "Path must start with '$'");
		else if (source[0] == '@')
			scope = PathScope.Local;
		else
			throw new PathParseException(index, "Path must start with '$' or '@'");

		index++; // consume $ or @

		while (index < source.Length)
		{
			segments.Add(ParseSegment(source, ref index));
		}

		return new JsonPath(scope, segments);
	}

	private static PathSegment ParseSegment(ReadOnlySpan<char> source, ref int index)
	{
		var selectors = new List<ISelector>();
		var isRecursive = false;
		var isShorthand = false;

		source.ConsumeWhitespace(ref index);

		if (source[index] == '[')
		{
			ParseBracketed(source, ref index, selectors);
		}
		else if (source[index] == '.')
		{
			index++; // consume .

			if (source[index] == '.')
			{
				isRecursive = true;
				index++; // consume second .

				if (source[index] == '[') 
					ParseBracketed(source, ref index, selectors);
				else if (source[index] == '*')
				{
					selectors.Add(new WildcardSelector());
					isRecursive = true;
					isShorthand = true;
					index++;
				}
				else
				{
					isRecursive = true;
					isShorthand = true;
					ParseName(source, ref index, selectors);
				}
			}
			else if (source[index] == '*')
			{
				selectors.Add(new WildcardSelector());
				isShorthand = true;
				index++;
			}
			else
			{
				isShorthand = true;
				ParseName(source, ref index, selectors);
			}
		}

		if (selectors.Count == 0)
			throw new PathParseException(index, "Could not find any valid selectors.");

		if (selectors.Count > 1 && isShorthand)
			throw new PathParseException(index, "Cannot have shorthand syntax with multiple selectors (something went very wrong).");

		return new PathSegment(selectors, isRecursive, isShorthand);
	}

	private static void ParseName(ReadOnlySpan<char> source, ref int index, List<ISelector> selectors)
	{
		var i = index;

		source.ConsumeWhitespace(ref i);

		while (i < source.Length && IsValidForPropertyName(source[i]))
		{
			i++;
		}

		if (index == i)
			throw new PathParseException(index, "Expected shorthand name selector but got no name");

		var name = source[index..i].ToString();
		selectors.Add(new NameSelector(name));
		index = i;
	}

	private static void ParseBracketed(ReadOnlySpan<char> source, ref int index, List<ISelector> selectors)
	{
		var done = false;
		index++; // consume [

		while (index < source.Length && !done)
		{
			source.ConsumeWhitespace(ref index);
			ISelector? selector = null;

			foreach (var parser in _parsers)
			{
				if (parser.TryParse(source, ref index, out selector)) break;
			}

			if (selector == null)
			{
				var sample = source[index..Math.Min(source.Length, index + 10)];
				var includeEllipsis = index < source.Length - 10;
				throw new PathParseException(index, $"Pattern '{sample.ToString()}{(includeEllipsis ? "..." : string.Empty)}' not recognized.");
			}

			selectors.Add(selector);

			source.ConsumeWhitespace(ref index);

			switch (source[index])
			{
				case ']':
					done = true;
					index++;
					break;
				case ',':
					index++;
					break;
				default:
					throw new PathParseException(index, "Expected ']' or ','.");
			}
		}

		if (!done)
			throw new PathParseException(index, "Unexpected end of input");
	}

	private static bool IsValidForPropertyName(char ch)
	{
		return ch.In('a'..('z' + 1)) ||
		       ch.In('A'..('Z' + 1)) ||
		       ch.In('0'..('9' + 1)) ||
		       ch.In('_') ||
		       ch.In(0x80..0x10FFFF);
	}
}