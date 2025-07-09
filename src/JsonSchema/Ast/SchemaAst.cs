using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Ast;

public record JsonSchemaNode(
    Uri BaseUri,
    JsonPointer SchemaLocation,
    JsonSchemaNodeType NodeType,
    Dictionary<string, object> Properties,
    List<JsonSchemaNode> Children,
    JsonPointer InstancePathFromParent = default,
    JsonPointer SchemaPathFromParent = default,
    Uri? ResolvedReference = null,
    JsonSchemaNode? ReferenceTarget = null
);

public enum JsonSchemaNodeType
{
    True,
    False,
    Object,
    Reference
}

public record BuildContext(
    Uri BaseUri,
    JsonPointer SchemaPath,
    Dictionary<Uri, JsonElement> SchemaResources,
    Dictionary<string, Uri> Anchors,
    Dictionary<(Uri, JsonPointer), JsonSchemaNode> Visited,
    Dictionary<Uri, JsonSchemaNode> ReferenceCache,
    JsonPointer InstancePath = default
);

public record ChildSpec(
    JsonPointer InstancePath,
    JsonPointer SchemaPath,
    JsonElement SubSchema
); 