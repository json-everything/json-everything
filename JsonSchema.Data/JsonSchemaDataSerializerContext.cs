using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema.Data;

[JsonSerializable(typeof(DataKeyword))]
[JsonSerializable(typeof(OptionalDataKeyword))]
[JsonSerializable(typeof(Uri))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(JsonPointer))]
[JsonSerializable(typeof(RelativeJsonPointer))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Dictionary<string, JsonNode>))]
internal partial class JsonSchemaDataSerializerContext : JsonSerializerContext;