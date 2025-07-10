using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public record ChildSpec(
	JsonPointer InstancePath,
	JsonPointer SchemaPath,
	JsonElement SubSchema
);