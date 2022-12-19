using System.Diagnostics.CodeAnalysis;
using System;

namespace Json.Path;

internal class SliceSelector : ISelector
{
	public int Start { get; set; }
	public int End { get; set; }
	public int Iterator { get; set; }
}

internal class SliceSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ISelector? selector)
	{
		var i = index;
		int start = 0,
			exclusiveEnd = int.MaxValue,
			iterator = 1;

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