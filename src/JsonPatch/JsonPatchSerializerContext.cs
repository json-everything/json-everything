using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Patch;

/// <summary>
/// A serializer context for this library.
/// </summary>
[JsonSerializable(typeof(JsonPatch))]
[JsonSerializable(typeof(PatchOperation))]
[JsonSerializable(typeof(PatchOperationJsonConverter.Model))]
[JsonSerializable(typeof(OperationType))]
[JsonSerializable(typeof(PatchResult))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonPointer))]
[JsonSerializable(typeof(List<PatchOperation>))]
internal partial class JsonPatchSerializerContext : JsonSerializerContext;