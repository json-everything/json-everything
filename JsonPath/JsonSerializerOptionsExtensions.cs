using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Path;

public static class JsonSerializerOptionsExtensions
{
	public static JsonSerializerOptions WithJsonPath(this JsonSerializerOptions options)
	{
		options.TypeInfoResolverChain.Add(JsonPathSerializerContext.Default);
		return options;
	}
}

[JsonSerializable(typeof(JsonPath))]
internal partial class JsonPathSerializerContext : JsonSerializerContext;