using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Json.Path;

internal class IndexSelector : ISelector
{
	public int Index { get; set; }

	public override string ToString()
	{
		return Index.ToString();
	}

	public void BuildString(StringBuilder builder)
	{
		builder.Append(Index);
	}
}

internal class IndexSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ISelector? selector)
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