using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Patch;

public static class JsonSerializerOptionsExtensions
{
	public static JsonSerializerOptions WithJsonPatch(this JsonSerializerOptions options)
	{
		if (!options.TryGetTypeInfo(typeof(JsonPointer), out _))
			options.WithJsonPointer();

		options.TypeInfoResolverChain.Add(JsonPatchSerializerContext.Default);
		return options;
	}
}

/// <summary>
/// A serializer context for this library.
/// </summary>
[JsonSerializable(typeof(JsonPatch))]
[JsonSerializable(typeof(PatchOperation))]
[JsonSerializable(typeof(OperationType))]
[JsonSerializable(typeof(PatchResult))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(List<PatchOperation>))]
[JsonSerializable(typeof(IReadOnlyList<PatchOperation>))]
internal partial class JsonPatchSerializerContext : JsonSerializerContext;