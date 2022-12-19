using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Json.Path;

internal class WildcardSelector : ISelector, IHaveShorthand
{
	public string ToShorthandString()
	{
		return ".*";
	}

	public void AppendShorthandString(StringBuilder builder)
	{
		builder.Append(".*");
	}

	public override string ToString()
	{
		return "*";
	}

	public void BuildString(StringBuilder builder)
	{
		builder.Append('*');
	}
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