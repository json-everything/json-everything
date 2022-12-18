using System;

namespace Json.Path;

internal class IndexSelector : ISelector
{
	public int Index { get; set; }
}

internal class IndexSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ISelector? selector)
	{
		if (!source.TryGetInt(ref index, out var value))
		{
			selector = null;
			return false;
		}

		selector = new IndexSelector { Index = value };
		return true;
	}
}