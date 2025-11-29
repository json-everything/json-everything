using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(SchemaValueType))]
[JsonSerializable(typeof(EvaluationResults))]
[JsonSerializable(typeof(JsonPointer))]
[JsonSerializable(typeof(JsonSchemaBuilder))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(HashSet<string>))]
internal partial class JsonSchemaSerializerContext : JsonSerializerContext;