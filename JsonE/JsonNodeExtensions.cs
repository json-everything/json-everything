using System.Data.Common;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE;

internal static class JsonNodeExtensions
{
	private static readonly JsonNode? _emptyString = string.Empty;
	private static readonly JsonNode? _zero = 0;
	private static readonly JsonNode? _false = false;

	public static bool IsTruthy(this JsonNode? node)
	{
		if (node is null) return false;
		if (node is JsonObject { Count: 0 }) return false;
		if (node is JsonArray { Count: 0 }) return false;
		if (node.IsEquivalentTo(_false)) return false;
		if (node.IsEquivalentTo(_zero)) return false;
		if (node.IsEquivalentTo(_emptyString)) return false;

		return true;
	}

	public static bool TryGetTemplate(this JsonNode? node, out JsonETemplate? template)
	{
		template = null;
		return node switch
		{
			JsonValue value when value.TryGetValue(out template) => true,
			_ => false
		};
	}
}