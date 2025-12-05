using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema.Data;

[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(IEnumerable<JsonNode>))]
internal partial class JsonSchemaDataSerializerContext : JsonSerializerContext;