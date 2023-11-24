using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class ValueAccessor
{
	private readonly IContextAccessorSegment[] _segments;
	private readonly string _asString;

	private ValueAccessor(IEnumerable<IContextAccessorSegment> segments, string asString)
	{
		_segments = segments.ToArray();
		_asString = asString;
	}

	internal static bool TryParse(ReadOnlySpan<char> source, ref int index, out ValueAccessor? accessor)
	{
		var i = index;
		if (!source.ConsumeWhitespace(ref i))
		{
			accessor = null;
			return false;
		}

		var segments = new List<IContextAccessorSegment>();

		while (i < source.Length)
		{
			if (!source.ConsumeWhitespace(ref i))
			{
				accessor = null;
				return false;
			}

			switch (source[i])
			{
				case '.':
					i++;
					if (!source.TryParseName(ref i, out var name))
						throw new TemplateException("Invalid name after dot accessor");

					segments.Add(new PropertySegment(name!, false));
					continue;
				case '[':
					i++;

					if (!source.ConsumeWhitespace(ref i))
					{
						accessor = null;
						return false;
					}

					if (!TryParseQuotedName(source, ref i, out var segment) &&
					    !TryParseSlice(source, ref i, out segment) &&
					    !TryParseIndex(source, ref i, out segment) &&
					    !TryParseExpression(source, ref i, out segment))
						throw new TemplateException("Cannot determine segment type");

					segments.Add(segment!);

					if (!source.ConsumeWhitespace(ref i))
					{
						accessor = null;
						return false;
					}

					if (source[i] != ']')
						throw new TemplateException("Missing closing ]");

					i++;

					continue;
			}

			break;
		}

		if (segments.Count == 0)
		{
			accessor = null;
			return false;
		}

		var asString = source[index..i].ToString();
		index = i;
		accessor = new ValueAccessor(segments, asString);
		return true;
	}

	private static bool TryParseQuotedName(ReadOnlySpan<char> source, ref int index, out IContextAccessorSegment? segment)
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
				segment = null;
				return false;
		}

		var done = false;
		var sb = new StringBuilder();
		while (i < source.Length && !done)
		{
			if (source[i] == quoteChar)
			{
				done = true;
				i++;
			}
			else
			{
				if (!source.EnsureValidNameCharacter(i))
				{
					segment = null;
					return false;
				}
				sb.Append(source[i]);
				i++;
			}
		}

		if (!done)
		{
			segment = null;
			return false;
		}

		index = i;
		segment = new PropertySegment(sb.ToString(), true);
		return true;

	}

	private static bool TryParseIndex(ReadOnlySpan<char> source, ref int index, out IContextAccessorSegment? segment)
	{
		if (!source.TryGetInt(ref index, out var i))
		{
			segment = null;
			return false;
		}

		segment = new IndexSegment(i);
		return true;
	}

	private static bool TryParseSlice(ReadOnlySpan<char> source, ref int index, out IContextAccessorSegment? segment)
	{
		var i = index;
		int? start = null, end = null, step = null;

		if (source.TryGetInt(ref i, out var value))
			start = value;

		if (!source.ConsumeWhitespace(ref i))
		{
			segment = null;
			return false;
		}

		if (source[i] != ':')
		{
			segment = null;
			return false;
		}

		i++; // consume :

		if (!source.ConsumeWhitespace(ref i))
		{
			segment = null;
			return false;
		}

		if (source.TryGetInt(ref i, out value))
			end = value;

		if (!source.ConsumeWhitespace(ref i))
		{
			segment = null;
			return false;
		}

		if (source[i] == ':')
		{
			i++; // consume :

			if (!source.ConsumeWhitespace(ref i))
			{
				segment = null;
				return false;
			}

			if (source.TryGetInt(ref i, out value))
				step = value;
		}

		index = i;
		segment = new SliceSegment(start, end, step);
		return true;
	}

	private static bool TryParseExpression(ReadOnlySpan<char> source, ref int i, out IContextAccessorSegment? segment)
	{
		if (!ExpressionParser.TryParse(source, ref i, out var expression))
		{
			segment = null;
			return false;
		}

		segment = new ExpressionSegment(expression!);
		return true;
	}

	internal JsonNode? Find(JsonNode? localContext, EvaluationContext fullContext)
	{
		var current = localContext;
		foreach (var segment in _segments)
		{
			if (!segment.TryFind(current, fullContext, out var value))
				throw new InterpreterException($"unknown context value {segment}");

			current = value;
		}

		return current;
	}

	public override string ToString() => _asString;
}