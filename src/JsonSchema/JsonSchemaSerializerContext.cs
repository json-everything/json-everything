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
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(Uri))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(List<string>))]
internal partial class JsonSchemaSerializerContext : JsonSerializerContext;