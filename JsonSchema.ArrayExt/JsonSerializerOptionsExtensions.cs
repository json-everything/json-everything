using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema.ArrayExt;

public static class JsonSerializerOptionsExtensions
{
	public static JsonSerializerOptions WithArrayExtVocab(this JsonSerializerOptions options)
	{
		if (!options.TryGetTypeInfo(typeof(JsonSchema), out _))
			options.WithJsonSchema();

		options.TypeInfoResolverChain.Add(JsonSchemaArrayExtSerializerContext.Default);
		return options;
	}
}

/// <summary>
/// A serializer context for this library.
/// </summary>
[JsonSerializable(typeof(UniqueKeysKeyword))]
[JsonSerializable(typeof(OrderingKeyword))]
[JsonSerializable(typeof(IEnumerable<JsonPointer>))]
[JsonSerializable(typeof(List<JsonPointer>))]
[JsonSerializable(typeof(IEnumerable<OrderingSpecifier>))]
[JsonSerializable(typeof(List<OrderingSpecifier>))]
[JsonSerializable(typeof(int))]
internal partial class JsonSchemaArrayExtSerializerContext : JsonSerializerContext;