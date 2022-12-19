using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Json.Path;

internal static class NodeExtensions
{
	public static bool TryGetValue<T>(this JsonNode? node, [NotNullWhen(true)] out T? value)
	{
		if (node == null)
		{
			value = default;
			return false;
		}

		var obj = node.GetValue<object>();
		if (obj is T objAsT)
		{
			value = objAsT;
			return true;
		}

		value = default;
		return false;
	}
}