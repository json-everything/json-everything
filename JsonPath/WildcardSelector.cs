using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Represents a wildcard selector, `[*]` or `.*`.
/// </summary>
public class WildcardSelector : ISelector, IHaveShorthand
{
	internal WildcardSelector(){}

	string IHaveShorthand.ToShorthandString()
	{
		return ".*";
	}

	void IHaveShorthand.AppendShorthandString(StringBuilder builder)
	{
		builder.Append(".*");
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return "*";
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
		if (node is JsonObject obj)
		{
			foreach (var member in obj)
			{
				yield return new Node(member.Value, match.Location.Append(member.Key));
			}
		}
		else if (node is JsonArray arr)
		{
			for (var i = 0; i < arr.Count; i++)
			{
				var member = arr[i];
				yield return new Node(member, match.Location.Append(i));
			}
		}
	}

	/// <summary>
	/// Builds a string using a string builder.
	/// </summary>
	/// <param name="builder">The string builder.</param>
	public void BuildString(StringBuilder builder)
	{
		builder.Append('*');
	}
}

internal class WildcardSelectorParser : ISelectorParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ISelector? selector, PathParsingOptions options)
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