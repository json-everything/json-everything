using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Extension methods for <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
	/// <summary>
	/// Adds serializer context information to the type resolver chain.
	/// </summary>
	/// <param name="options">The options.</param>
	/// <returns>The same options.</returns>
	/// <remarks>
	/// Also adds the context for <see cref="JsonPointer"/> and <see cref="JsonSchema"/>.
	/// </remarks>
	public static JsonSerializerOptions WithArrayExtVocab(this JsonSerializerOptions options)
	{
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