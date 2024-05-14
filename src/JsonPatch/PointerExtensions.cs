using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Patch;

internal static class PointerExtensions
{
	public static bool EvaluateAndGetParent(this JsonPointer pointer, JsonNode? node, out JsonNode? target)
	{
		if (pointer == JsonPointer.Empty)
		{
			target = node?.Parent;
			return target != null;
		}
#if NETSTANDARD2_0
		var parentPointer = pointer.GetAncestor(1);
#else
		var parentPointer = pointer[..^1];
#endif
		return parentPointer.TryEvaluate(node, out target);
	}
}