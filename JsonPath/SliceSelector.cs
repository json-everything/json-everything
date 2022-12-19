using System.Diagnostics.CodeAnalysis;
using System;
using System.Text;

namespace Json.Path;

internal class SliceSelector : ISelector
{
	public int? Start { get; set; }
	public int? End { get; set; }
	public int? Iterator { get; set; }

	public override string ToString()
	{
		return Iterator.HasValue
			? $"{Start}:{End}:{Iterator}"
			: $"{Start}:{End}";
	}

	public void BuildString(StringBuilder builder)
	{
		builder.Append(Start);
		builder.Append(':');
		builder.Append(End);
		if (Iterator.HasValue)
		{
			builder.Append(':');
			builder.Append(Iterator);
		}
	}
}

internal class SliceSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ISelector? selector)
	{
		var i = index;
		int? start = null, exclusiveEnd = null, iterator = null;

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
				iterator = value;
		}

		index = i;
		selector = new SliceSelector
		{
			Start = start,
			End = exclusiveEnd,
			Iterator = iterator
		};
		return true;
	}
}