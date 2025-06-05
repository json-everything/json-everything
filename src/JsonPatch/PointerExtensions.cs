using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Patch;

internal static class PointerExtensions
{
	public static bool EvaluateAndGetParent(this JsonPointer_Old pointerOld, JsonNode? node, out JsonNode? target)
	{
		if (pointerOld == JsonPointer_Old.Empty)
		{
			target = node?.Parent;
			return target != null;
		}
#if NETSTANDARD2_0
		var parentPointer = pointerOld.GetAncestor(1);
#else
		var parentPointer = pointerOld[..^1];
#endif
		return parentPointer.TryEvaluate(node, out target);
	}
}