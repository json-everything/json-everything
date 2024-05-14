using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Represents an index selector.
/// </summary>
public class IndexSelector : ISelector
{
	/// <summary>
	/// Gets the index.
	/// </summary>
	public int Index { get; }

	internal IndexSelector(int index)
	{
		Index = index;
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Index.ToString();
	}

	/// <summary>
	/// Evaluates the selector.
	/// </summary>
	/// <param name="match">The node to evaluate.</param>
	/// <param name="rootNode">The root node (typically used by filter selectors, e.g. `$[?@foo &lt; $.bar]`)</param>
	/// <returns>
	/// A collection of nodes.
	///
	/// Semantically, this is a nodelist, but leaving as IEnumerable&lt;Node&gt; allows for deferred execution.
	/// </returns>
	public IEnumerable<Node> Evaluate(Node match, JsonNode? rootNode)
	{
		var node = match.Value;
		if (node is not JsonArray arr) yield break;
		if (Index >= arr.Count) yield break;

		if (Index < 0)
		{
			var adjusted = arr.Count + Index;
			if (adjusted < 0) yield break;
			yield return new Node(arr[adjusted], match.Location!.Append(adjusted));
		}
		else yield return new Node(arr[Index], match.Location!.Append(Index));
	}

	/// <summary>
	/// Builds a string using a string builder.
	/// </summary>
	/// <param name="builder">The string builder.</param>
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