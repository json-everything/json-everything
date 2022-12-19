using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Path;

internal class WildcardSelector : ISelector
{

}

internal class WildcardSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ISelector? selector)
	{
		if (source[index] != '*')
		{
			selector = null;
			return false;
		}

		selector = new WildcardSelector();
		index++;
		return true;
	}
}