using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

	public static JsonPath Parse(ReadOnlySpan<char> source, ref int index, PathParsingOptions options, bool requireGlobal = false)
	{
		if (source.Length == 0)
			throw new PathParseException(0, "Input string is empty");

		var segments = new List<PathSegment>();
		PathScope scope;

		if (source[index] == '$')
			scope = PathScope.Global;
		else if (requireGlobal)
			throw new PathParseException(index, "Path must start with '$'");
		else if (source[index] == '@')
			scope = PathScope.Local;
		else
			throw new PathParseException(index, "Path must start with '$' or '@'");

		index++; // consume $ or @

		while (index < source.Length)
		{
			segments.Add(ParseSegment(source, ref index, options));
		}

		return new JsonPath(scope, segments);
	}

	public static bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out JsonPath? path, PathParsingOptions options, bool requireGlobal = false)
	{
		if (index >= source.Length)
		{
			path = null;
			return false;
		}

		var i = index;

		//if (!source.ConsumeWhitespace(ref index))
		//{
		//	path = null;
		//	return false;
		//}

		var segments = new List<PathSegment>();
		PathScope scope;

		if (source[i] == '$')
			scope = PathScope.Global;
		else if (requireGlobal)
		{
			path = null;
			return false;
		}
		else if (source[i] == '@')
			scope = PathScope.Local;
		else
		{
			path = null;
			return false;
		}

		i++; // consume $ or @

		while (i < source.Length)
		{
			if (!TryParseSegment(source, ref i, out var pathSegment, options)) break;

			segments.Add(pathSegment);
		}

		path = new JsonPath(scope, segments);
		index = i;
		return true;
	}

	private static PathSegment ParseSegment(ReadOnlySpan<char> source, ref int index, PathParsingOptions options)
	{
		var selectors = new List<ISelector>();
		var isRecursive = false;
		var isShorthand = false;

		if (options.TolerateExtraWhitespace)
		{
			if (!source.ConsumeWhitespace(ref index))
				throw new PathParseException(index, "Unexpected end of input");
		}
		else if (index == source.Length)
			throw new PathParseException(index, "Unexpected end of input");

		if (source[index] == '[')
			ParseBracketed(source, ref index, selectors, options);
		else if (source[index] == '.')
		{
			index++; // consume .

			if (source[index] == '.')
			{
				isRecursive = true;
				index++; // consume second .

				if (options.TolerateExtraWhitespace)
				{
					if (!source.ConsumeWhitespace(ref index))
						throw new PathParseException(index, "Unexpected end of input");
				}
				else if (index == source.Length)
					throw new PathParseException(index, "Unexpected end of input");

				if (source[index] == '[') 
					ParseBracketed(source, ref index, selectors, options);
				else if (source[index] == '*')
				{
					selectors.Add(new WildcardSelector());
					isShorthand = true;
					index++; // consume *
				}
				else
				{
					isShorthand = true;
					ParseName(source, ref index, selectors, options);
				}
			}
			else if (source[index] == '*')
			{
				selectors.Add(new WildcardSelector());
				isShorthand = true;
				index++; // consume *
			}
			else
			{
				isShorthand = true;
				ParseName(source, ref index, selectors, options);
			}
		}

		if (selectors.Count == 0)
			throw new PathParseException(index, "Could not find any valid selectors.");

		if (selectors.Count > 1 && isShorthand)
			throw new PathParseException(index, "Cannot have shorthand syntax with multiple selectors (something went very wrong).");

		return new PathSegment(selectors, isRecursive, isShorthand);
	}

	private static bool TryParseSegment(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out PathSegment? segment, PathParsingOptions options)
	{
		var selectors = new List<ISelector>();
		var isRecursive = false;
		var isShorthand = false;

		var i = index;

		if (options.TolerateExtraWhitespace && !source.ConsumeWhitespace(ref i) || i == source.Length)
		{
			segment = null;
			return false;
		}

		if (source[i] == '[')
		{
			if (!TryParseBracketed(source, ref i, selectors, options))
			{
				segment = null;
				return false;
			}
		}
		else if (source[i] == '.')
		{
			i++; // consume .

			if (source[i] == '.')
			{
				isRecursive = true;
				i++; // consume second .

				if (options.TolerateExtraWhitespace && !source.ConsumeWhitespace(ref i) || i == source.Length)
				{
					segment = null;
					return false;
				}

				if (source[i] == '[')
				{
					if (!TryParseBracketed(source, ref i, selectors, options))
					{
						segment = null;
						return false;
					}
				}
				else if (source[i] == '*')
				{
					selectors.Add(new WildcardSelector());
					isRecursive = true;
					isShorthand = true;
					i++;
				}
				else
				{
					isRecursive = true;
					isShorthand = true;
					if (!TryParseName(source, ref i, selectors, options))
					{
						segment = null;
						return false;
					}
				}
			}
			else if (source[i] == '*')
			{
				selectors.Add(new WildcardSelector());
				isShorthand = true;
				i++;
			}
			else
			{
				isShorthand = true;
				if (!TryParseName(source, ref i, selectors, options))
				{
					segment = null;
					return false;
				}
			}
		}

		if (selectors.Count == 0)
		{
			segment = null;
			return false;
		}

		if (selectors.Count > 1 && isShorthand)
		{
			segment = null;
			return false;
		}

		index = i;
		segment = new PathSegment(selectors, isRecursive, isShorthand);
		return true;
	}

	private static void ParseName(ReadOnlySpan<char> source, ref int index, List<ISelector> selectors, PathParsingOptions options)
	{
		if (!source.TryParseName(ref index, out var name, options)) return;

		selectors.Add(new NameSelector(name));
	}

	private static bool TryParseName(this ReadOnlySpan<char> source, ref int index, List<ISelector> selectors, PathParsingOptions options)
	{
		if (!source.TryParseName(ref index, out var name, options)) return false;

		selectors.Add(new NameSelector(name));
		return true;
	}

	private static void ParseBracketed(ReadOnlySpan<char> source, ref int index, List<ISelector> selectors, PathParsingOptions options)
	{
		var done = false;
		index++; // consume [

		while (index < source.Length && !done)
		{
			if (!source.ConsumeWhitespace(ref index))
				throw new PathParseException(index, "Unexpected end of input");

			ISelector? selector = null;

			foreach (var parser in _parsers)
			{
				if (parser.TryParse(source, ref index, out selector, options)) break;
			}

			if (selector == null)
			{
				var sample = source[index..Math.Min(source.Length, index + 10)];
				var includeEllipsis = index < source.Length - 10;
				throw new PathParseException(index, $"Pattern '{sample.ToString()}{(includeEllipsis ? "..." : string.Empty)}' not recognized.");
			}

			selectors.Add(selector);

			if (!source.ConsumeWhitespace(ref index))
				throw new PathParseException(index, "Unexpected end of input");

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

	private static bool TryParseBracketed(ReadOnlySpan<char> source, ref int index, List<ISelector> selectors, PathParsingOptions options)
	{
		var done = false;
		index++; // consume [

		while (index < source.Length && !done)
		{
			if (options.TolerateExtraWhitespace && !source.ConsumeWhitespace(ref index) || index == source.Length) return false;

			ISelector? selector = null;

			foreach (var parser in _parsers)
			{
				if (parser.TryParse(source, ref index, out selector, options)) break;
			}

			if (selector == null) return false;

			selectors.Add(selector);

			if (options.TolerateExtraWhitespace && !source.ConsumeWhitespace(ref index) || index == source.Length) return false;

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
					return false;
			}
		}

		return done;
	}
}