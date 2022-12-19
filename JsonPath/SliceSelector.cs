using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

internal class SliceSelector : ISelector
{
	public int? Start { get; set; }
	public int? End { get; set; }
	public int? Step { get; set; }

	public override string ToString()
	{
		return Step.HasValue
			? $"{Start}:{End}:{Step}"
			: $"{Start}:{End}";
	}

	public IEnumerable<PathMatch> Evaluate(JsonNode? node)
	{
		if (node is not JsonArray arr) yield break;
		if (Step == 0) yield break;

		var step = Step ?? 1;
		var (lower, upper) = Bounds(Start ?? 0, End ?? int.MaxValue, step, arr.Count);

		if (step > 0)
		{
			var i = lower;
			while (i < upper)
			{
				yield return new PathMatch(arr[i], null);
				i += step;
			}
		}
		else
		{
			var i = upper;
			while (lower < i)
			{
				yield return new PathMatch(arr[i], null);
				i += step;
			}
		}
	}

	public void BuildString(StringBuilder builder)
	{
		builder.Append(Start);
		builder.Append(':');
		builder.Append(End);
		if (Step.HasValue)
		{
			builder.Append(':');
			builder.Append(Step);
		}
	}

	private static int Normalize(int i, int length)
	{
		return i >= 0 ? i : length + i;
	}

	private static (int lower, int upper) Bounds(int start, int end, int step, int length)
	{
		start = Normalize(start, length);
		end = Normalize(end, length);

		return step >= 0
			? (Math.Min(Math.Max(start, 0), length), Math.Min(Math.Max(end, 0), length))
			: (Math.Min(Math.Max(start, -1), length - 1), Math.Min(Math.Max(end, -1), length - 1));
	}
}

internal class SliceSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ISelector? selector)
	{
		var i = index;
		int? start = null, exclusiveEnd = null, step = null;

		if (source.TryGetInt(ref i, out var value)) 
			start = value;

		source.ConsumeWhitespace(ref i);

		if (source[i] != ':')
		{
			selector = null;
			return false;
		}

		i++; // consume :

		source.ConsumeWhitespace(ref i);

		if (source.TryGetInt(ref i, out value)) 
			exclusiveEnd = value;

		source.ConsumeWhitespace(ref i);
		
		if (source[i] == ':')
		{
			i++; // consume :

			source.ConsumeWhitespace(ref i);
			
			if (source.TryGetInt(ref i, out value))
				step = value;
		}

		index = i;
		selector = new SliceSelector
		{
			Start = start,
			End = exclusiveEnd,
			Step = step
		};
		return true;
	}
}