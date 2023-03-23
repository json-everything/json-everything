using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal abstract class PathValue
{
	public JsonNode? TryGetJson() =>
		this switch
		{
			JsonPathValue v => v.Value ?? JsonNull.SignalNode,
			NodeListPathValue n => n.Value.TryGetSingleValue(),
			_ => null
		};

	public static implicit operator PathValue?(JsonNode? node)
	{
		return node == null ? null : new JsonPathValue { Value = node };
	}

	public static implicit operator PathValue?(bool? node)
	{
		return node == null ? null : new LogicalPathValue { Value = node.Value };
	}

	public static implicit operator PathValue?(NodeList? node)
	{
		return node == null ? null : new NodeListPathValue { Value = node };
	}
}

internal class JsonPathValue : PathValue
{
	public JsonNode? Value { get; set; }
}

internal class LogicalPathValue : PathValue
{
	public bool Value { get; set; }
}

internal class NodeListPathValue : PathValue
{
	public NodeList Value { get; set; } = NodeList.Empty;
}