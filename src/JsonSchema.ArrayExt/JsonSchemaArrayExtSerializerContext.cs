using System.Text.Json.Serialization;
using Json.Pointer;
using Json.Schema.ArrayExt.Keywords;

namespace Json.Schema.ArrayExt;

[JsonSerializable(typeof(UniqueKeysKeyword))]
[JsonSerializable(typeof(OrderingKeyword))]
[JsonSerializable(typeof(JsonPointer))]
[JsonSerializable(typeof(OrderingSpecifier))]
[JsonSerializable(typeof(OrderingSpecifier[]))]
[JsonSerializable(typeof(int))]
internal partial class JsonSchemaArrayExtSerializerContext : JsonSerializerContext;