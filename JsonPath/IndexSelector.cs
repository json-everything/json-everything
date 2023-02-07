using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

internal class IndexSelector : ISelector
{
	public int Index { get; }

	public IndexSelector(int index)
	{
		Index = index;
	}

	public override string ToString()
	{
		return Index.ToString();
	}

	public IEnumerable<Node> Evaluate(Node match, JsonNode? rootNode)
	{
		var node = match.Value;
		if (node is not JsonArray arr) yield break;
		if (Index >= arr.Count) yield break;

		if (Index < 0)
		{
			var adjusted = arr.Count + Index;
			if (adjusted < 0) yield break;
			yield return new Node(arr[adjusted], match.Location.Append(adjusted));
		}
		else yield return new Node(arr[Index], match.Location.Append(Index));
	}

	public void BuildString(StringBuilder builder)
	{
		builder.Append(Index);
	}
}

internal class IndexSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ISelector? selector, PathParsingOptions options)
	{
		if (!source.TryGetInt(ref index, out var value))
		{
			selector = null;
			return false;
		}

		selector = new IndexSelector(value);
		return true;
	}
}