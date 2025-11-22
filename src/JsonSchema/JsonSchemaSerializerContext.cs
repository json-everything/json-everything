using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// A serializer context for this library.
/// </summary>
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(SchemaValueType))]
[JsonSerializable(typeof(EvaluationResults))]
[JsonSerializable(typeof(JsonPointer))]
[JsonSerializable(typeof(uint))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(Uri))]
internal partial class JsonSchemaSerializerContext : JsonSerializerContext;