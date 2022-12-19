using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

internal class IndexSelector : ISelector
{
	public int Index { get; set; }

	public override string ToString()
	{
		return Index.ToString();
	}

	public IEnumerable<PathMatch> Evaluate(JsonNode? node)
	{
		if (node is not JsonArray arr) yield break;
		if (Index >= arr.Count) yield break;

		if (Index < 0)
		{
			var adjusted = arr.Count - Index;
			if (adjusted < 0) yield break;
			yield return new PathMatch(arr[adjusted], null);
		}
		else yield return new PathMatch(arr[Index], null);
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