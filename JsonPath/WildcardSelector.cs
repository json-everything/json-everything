using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;

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

	public IEnumerable<PathMatch> Evaluate(JsonNode? node)
	{
		if (node is JsonObject obj)
		{
			foreach (var member in obj)
			{
				yield return new PathMatch(member.Value, null);
			}
		}
		else if (node is JsonArray arr)
		{
			foreach (var member in arr)
			{
				yield return new PathMatch(member, null);
			}
		}
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