using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Path;

public class NodeList : IReadOnlyList<Node>
{
	private readonly List<Node> _nodes;

	public static readonly NodeList Empty = new(Enumerable.Empty<Node>());

	public int Count => _nodes.Count;

	public Node this[int index] => _nodes[index];

	public NodeList(IEnumerable<Node> nodes)
	{
		_nodes = nodes.ToList();
	}

	public IEnumerator<Node> GetEnumerator()
	{
		return _nodes.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static implicit operator NodeList(JsonNode node)
	{
		if (node.TryGetValue<NodeList>(out var nodeList)) return nodeList;

		return new NodeList(new Node[] { new(node, null) });
	}

	public static implicit operator JsonNode(NodeList list)
	{
		return JsonValue.Create(list)!;
	}
}