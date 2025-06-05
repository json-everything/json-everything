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
		var parentPointer = pointer.GetParent();
		if (parentPointer == null)
		{
			target = null;
			return false;
		}
		return parentPointer.Value.TryEvaluate(node, out target);
	}
}