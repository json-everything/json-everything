using System.Collections.Generic;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema.ArrayExt;

[JsonSerializable(typeof(UniqueKeysKeyword))]
[JsonSerializable(typeof(OrderingKeyword))]
[JsonSerializable(typeof(IEnumerable<JsonPointer>))]
[JsonSerializable(typeof(List<JsonPointer>))]
[JsonSerializable(typeof(IEnumerable<OrderingSpecifier>))]
[JsonSerializable(typeof(List<OrderingSpecifier>))]
[JsonSerializable(typeof(int))]
internal partial class JsonSchemaArrayExtSerializerContext : JsonSerializerContext;