using System.Text.Json.Serialization;

namespace Json.Schema.CodeGeneration;

[JsonSerializable(typeof(JsonSchema))]
internal partial class CodeGenSerializerContext : JsonSerializerContext;