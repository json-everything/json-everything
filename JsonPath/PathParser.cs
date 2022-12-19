using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Json.Path;

internal static class PathParser
{
	private static readonly ISelectorParser[] _parsers =
	{
		new SliceSelectorParser(),
		new IndexSelectorParser(),
		new NameSelectorParser(),
		new WildcardSelectorParser()
	};

	public static JsonPath Parse(ReadOnlySpan<char> source, bool requireGlobal = false)
	{
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

		source.ConsumeWhitespace(ref index);

		if (source[index] == '[')
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
				throw new PathParseException(index, "Reached end of input");

			index++; // consume ]
		}

		// TODO: handle dot-notation

		if (selectors.Count == 0)
			throw new PathParseException(index, "Could not find any valid selectors.");

		return new PathSegment { Selectors = selectors.ToArray() };
	}
}

public static class NodeExtensions
{
	public static bool TryGetValue<T>(this JsonNode? node, [NotNullWhen(true)] out T? value)
	{
		if (node == null)
		{
			value = default;
			return false;
		}

		var obj = node.GetValue<object>();
		if (obj is T objAsT)
		{
			value = objAsT;
			return true;
		}

		value = default;
		return false;
	}
}