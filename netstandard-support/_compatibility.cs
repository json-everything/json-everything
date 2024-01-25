#if NET8_0_OR_GREATER

using System.Text.Json.Nodes;

namespace Json.More
{
	internal static class JsonNodeExtensions
	{
		internal static JsonNode? Copy(this JsonNode? a)
		{
			return a?.DeepClone();
		}
	}
}

#endif