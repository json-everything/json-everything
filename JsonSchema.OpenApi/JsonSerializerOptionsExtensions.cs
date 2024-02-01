using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema.OpenApi;

public static class JsonSerializerOptionsExtensions
{
	public static JsonSerializerOptions WithOpenApiVocab(this JsonSerializerOptions options)
	{
		options.TypeInfoResolverChain.Add(JsonSchemaOpenApiSerializerContext.Default);
		return options;
	}
}

[JsonSerializable(typeof(ExampleKeyword))]
[JsonSerializable(typeof(DiscriminatorKeyword))]
[JsonSerializable(typeof(ExternalDocsKeyword))]
[JsonSerializable(typeof(XmlKeyword))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(IReadOnlyDictionary<string, string>))]
internal partial class JsonSchemaOpenApiSerializerContext : JsonSerializerContext;