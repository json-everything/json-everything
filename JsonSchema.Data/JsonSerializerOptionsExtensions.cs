using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Path;
using Json.Pointer;

namespace Json.Schema.Data;

public static class JsonSerializerOptionsExtensions
{
	public static JsonSerializerOptions WithDataVocab(this JsonSerializerOptions options)
	{
		options.WithJsonPath();
		options.WithJsonSchema();
		options.TypeInfoResolverChain.Add(JsonSchemaDataSerializerContext.Default);
		return options;
	}

	internal static readonly JsonSerializerOptions DefaultSerializerOptions =
		new JsonSerializerOptions().WithDataVocab();
}

[JsonSerializable(typeof(DataKeyword))]
[JsonSerializable(typeof(OptionalDataKeyword))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, JsonNode>))]
[JsonSerializable(typeof(Uri))]
[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(JsonPointer))]
[JsonSerializable(typeof(RelativeJsonPointer))]
internal partial class JsonSchemaDataSerializerContext : JsonSerializerContext;