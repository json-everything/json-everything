using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema.OpenApi;

[JsonSerializable(typeof(ExampleKeyword))]
[JsonSerializable(typeof(DiscriminatorKeyword))]
[JsonSerializable(typeof(ExternalDocsKeyword))]
[JsonSerializable(typeof(XmlKeyword))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(string))]
internal partial class JsonSchemaOpenApiSerializerContext : JsonSerializerContext;