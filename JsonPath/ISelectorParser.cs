using System;

namespace Json.Path;

internal interface ISelectorParser
{
	bool TryParse(ReadOnlySpan<char> source, ref int index, out ISelector? selector);
}