using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public record BuildContext(
	Uri BaseUri,
	JsonPointer SchemaPath,
	Dictionary<Uri, JsonElement> SchemaResources,
	Dictionary<string, Uri> Anchors,
	Dictionary<(Uri, JsonPointer), JsonSchemaNode> Visited,
	Dictionary<Uri, JsonSchemaNode> ReferenceCache,
	JsonPointer InstancePath = default,
	JsonElement? CurrentSchema = null
);