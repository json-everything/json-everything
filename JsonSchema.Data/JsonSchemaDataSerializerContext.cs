using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Path;
using Json.Pointer;

namespace Json.Schema.Data;

[JsonSerializable(typeof(DataKeyword))]
[JsonSerializable(typeof(OptionalDataKeyword))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, JsonNode>))]
[JsonSerializable(typeof(Uri))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(JsonPointer))]
[JsonSerializable(typeof(RelativeJsonPointer))]
internal partial class JsonSchemaDataSerializerContext : JsonSerializerContext;