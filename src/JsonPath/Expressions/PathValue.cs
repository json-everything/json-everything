using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal abstract class PathValue
{
	public bool TryGetJson(out JsonNode? node)
	{
		switch (this)
		{
			case JsonPathValue v:
				node = v.Value;
				return true;
			case NodeListPathValue n:
				return n.Value.TryGetSingleValue(out node);
			default:
				node = null;
				return false;
		}
	}

	public static implicit operator PathValue(JsonNode? node)
	{
		return new JsonPathValue { Value = node };
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