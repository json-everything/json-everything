using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// A collection of nodes, generally resulting from an operation or query.
/// </summary>
public class NodeList : IReadOnlyList<Node>
{
	private readonly List<Node> _nodes;

	/// <summary>
	/// An empty nodelist.
	/// </summary>
	public static readonly NodeList Empty = new(Enumerable.Empty<Node>());

	/// <summary>Gets the number of elements in the collection.</summary>
	/// <returns>The number of elements in the collection.</returns>
	public int Count => _nodes.Count;

	/// <summary>Gets the element at the specified index in the read-only list.</summary>
	/// <param name="index">The zero-based index of the element to get.</param>
	/// <returns>The element at the specified index in the read-only list.</returns>
	public Node this[int index] => _nodes[index];

	/// <summary>
	/// Creates a new nodelist.
	/// </summary>
	/// <param name="nodes">The nodes.</param>
	public NodeList(IEnumerable<Node> nodes)
	{
		_nodes = nodes.ToList();
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	public IEnumerator<Node> GetEnumerator()
	{
		return _nodes.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	///// <summary>
	///// Implicitly converts from <see cref="JsonNode"/> to <see cref="NodeList"/>.
	///// </summary>
	///// <param name="node">A `JsonNode`.</param>
	///// <remarks>
	///// If the `JsonNode`'s underlying value is a nodelist, that nodelist is returned
	///// rather than a new nodelist being created.
	///// </remarks>
	//public static implicit operator NodeList(JsonNode node)
	//{
	//	if (node.TryGetValue<NodeList>(out var nodeList)) return nodeList;

	//	return new NodeList(new Node[] { new(node, null) });
	//}

	///// <summary>
	///// Implicitly converts from <see cref="NodeList"/> to <see cref="JsonNode"/>
	///// </summary>
	///// <param name="list">The nodelist.</param>
	//public static implicit operator JsonNode(NodeList list)
	//{
	//	return JsonValue.Create(list)!;
	//}
}