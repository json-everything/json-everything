using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Patch;

/// <summary>
/// Extension methods for <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
	/// <summary>
	/// Adds serializer context information to the type resolver chain.
	/// </summary>
	/// <param name="options">The options.</param>
	/// <returns>The same options.</returns>
	/// <remarks>
	/// Also adds the context for <see cref="JsonPointer"/>.
	/// </remarks>
	public static JsonSerializerOptions WithJsonPatch(this JsonSerializerOptions options)
	{
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
[JsonSerializable(typeof(JsonPointer))]
[JsonSerializable(typeof(List<PatchOperation>))]
[JsonSerializable(typeof(IReadOnlyList<PatchOperation>))]
internal partial class JsonPatchSerializerContext : JsonSerializerContext;