using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Pointer;

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
	public static JsonSerializerOptions WithJsonPointer(this JsonSerializerOptions options)
	{
		options.TypeInfoResolverChain.Add(JsonPointerSerializerContext.Default);
		return options;
	}
}

[JsonSerializable(typeof(JsonPointer))]
[JsonSerializable(typeof(RelativeJsonPointer))]
internal partial class JsonPointerSerializerContext : JsonSerializerContext;