using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema.ArrayExt;

[JsonSerializable(typeof(UniqueKeysKeyword))]
[JsonSerializable(typeof(OrderingKeyword))]
[JsonSerializable(typeof(JsonPointer_Old))]
[JsonSerializable(typeof(OrderingSpecifier))]
[JsonSerializable(typeof(int))]
internal partial class JsonSchemaArrayExtSerializerContext : JsonSerializerContext;