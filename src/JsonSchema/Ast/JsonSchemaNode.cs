using System;
using System.Collections.Generic;
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