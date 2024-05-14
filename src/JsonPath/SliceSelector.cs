using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Represents a slice selector in the form `start : end : step`.
/// </summary>
public class SliceSelector : ISelector
{
	/// <summary>
	/// Gets the start.
	/// </summary>
	public int? Start { get; }
	/// <summary>
	/// Gets the end.
	/// </summary>
	public int? End { get; }
	/// <summary>
	/// Gets the step.
	/// </summary>
	public int? Step { get; }

	internal SliceSelector(int? start, int? end, int? step)
	{
		Start = start;
		End = end;
		Step = step;
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Step.HasValue
			? $"{Start}:{End}:{Step}"
			: $"{Start}:{End}";
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
		if (node is not JsonArray arr) yield break;
		if (Step == 0) yield break;

		var step = Step ?? 1;
		var start = Start ?? (step >= 0 ? 0 : arr.Count);
		var end = End ?? (step >= 0 ? arr.Count : -arr.Count - 1);
		var (lower, upper) = Bounds(start, end, step, arr.Count);

		if (step > 0)
		{
			var i = lower;
			while (i < upper)
			{
				yield return new Node(arr[i], match.Location!.Append(i));
				i += step;
				if (i < 0) break; // overflow
			}
		}
		else
		{
			var i = upper;
			while (lower < i)
			{
				yield return new Node(arr[i], match.Location!.Append(i));
				i += step;
				if (i < 0) break; // overflow
			}
		}
	}

	/// <summary>
	/// Builds a string using a string builder.
	/// </summary>
	/// <param name="builder">The string builder.</param>
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

		int lower, upper;

		if (step >= 0)
		{
			lower = Math.Min(Math.Max(start, 0), length);
			upper = Math.Min(Math.Max(end, 0), length);
		}
		else
		{
			upper = Math.Min(Math.Max(start, -1), length - 1);
			lower = Math.Min(Math.Max(end, -1), length - 1);
		}

		return (lower, upper);
	}
}

internal class SliceSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ISelector? selector, PathParsingOptions options)
	{
		var i = index;
		int? start = null, end = null, step = null;

		if (source.TryGetInt(ref i, out var value))
			start = value;

		if (!source.ConsumeWhitespace(ref i))
		{
			selector = null;
			return false;
		}

		if (source[i] != ':')
		{
			selector = null;
			return false;
		}

		i++; // consume :

		if (!source.ConsumeWhitespace(ref i))
		{
			selector = null;
			return false;
		}

		if (source.TryGetInt(ref i, out value))
			end = value;

		if (!source.ConsumeWhitespace(ref i))
		{
			selector = null;
			return false;
		}

		if (source[i] == ':')
		{
			i++; // consume :

			if (!source.ConsumeWhitespace(ref i))
			{
				selector = null;
				return false;
			}

			if (source.TryGetInt(ref i, out value))
				step = value;
		}

		index = i;
		selector = new SliceSelector(start, end, step);
		return true;
	}
}