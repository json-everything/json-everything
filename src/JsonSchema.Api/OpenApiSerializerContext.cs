using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Schema.Api.OpenApi;

namespace Json.Schema.Api;

/// <summary>
/// A serializer context for this library.
/// </summary>
[JsonSerializable(typeof(OpenApiDocument))]
[JsonSerializable(typeof(OpenApiInfo))]
[JsonSerializable(typeof(ContactInfo))]
[JsonSerializable(typeof(LicenseInfo))]
[JsonSerializable(typeof(Server))]
[JsonSerializable(typeof(ServerVariable))]
[JsonSerializable(typeof(PathCollection))]
[JsonSerializable(typeof(PathItem))]
[JsonSerializable(typeof(Operation))]
[JsonSerializable(typeof(Parameter))]
[JsonSerializable(typeof(RequestBody))]
[JsonSerializable(typeof(Response))]
[JsonSerializable(typeof(ResponseCollection))]
[JsonSerializable(typeof(MediaType))]
[JsonSerializable(typeof(Encoding))]
[JsonSerializable(typeof(Header))]
[JsonSerializable(typeof(Example))]
[JsonSerializable(typeof(Link))]
[JsonSerializable(typeof(Callback))]
[JsonSerializable(typeof(ComponentCollection))]
[JsonSerializable(typeof(SecurityScheme))]
[JsonSerializable(typeof(SecurityRequirement))]
[JsonSerializable(typeof(OAuthFlow))]
[JsonSerializable(typeof(OAuthFlowCollection))]
[JsonSerializable(typeof(Tag))]
[JsonSerializable(typeof(ExternalDocumentation))]
[JsonSerializable(typeof(RuntimeExpression))]
[JsonSerializable(typeof(ParameterLocation))]
[JsonSerializable(typeof(ParameterStyle))]
[JsonSerializable(typeof(SecuritySchemeLocation))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class ApiSerializerContext : JsonSerializerContext;
