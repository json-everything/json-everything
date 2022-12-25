using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Defines a path segment selector.
/// </summary>
public interface ISelector
{
	/// <summary>
	/// Evaluates the selector.
	/// </summary>
	/// <param name="node">The node to evaluate.</param>
	/// <param name="rootNode">The root node (typically used by filter selectors, e.g. `$[?@foo < $.bar]` </param>
	/// <returns>
	/// A collection of nodes.
	///
	/// Semantically, this is a nodelist, but leaving as `IEnumerable<Node>` allows for deferred execution.
	/// </returns>
	IEnumerable<Node> Evaluate(Node node, JsonNode? rootNode);

	void BuildString(StringBuilder builder);
}