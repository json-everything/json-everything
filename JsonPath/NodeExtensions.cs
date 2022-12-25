using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Json.Path;

public static class NodeExtensions
{
	public static bool TryGetValue<T>(this JsonNode? node, [NotNullWhen(true)] out T? value)
	{
		if (node is not JsonValue val)
		{
			value = default;
			return false;
		}

		var obj = val.GetValue<object>();
		if (obj is T objAsT)
		{
			value = objAsT;
			return true;
		}

		value = default;
		return false;
	}

	public static JsonNode? TryGetSingleValue(this JsonNode? node)
	{
		if (!node.TryGetValue<NodeList>(out var nodeList)) return node;

		if (nodeList.Count == 1) return nodeList[0].Value;

		return null;
	}

	public static JsonNode? TryGetSingleValue(this NodeList nodeList)
	{
		return nodeList.Count == 1
			? nodeList[0].Value
			: null;
	}
}