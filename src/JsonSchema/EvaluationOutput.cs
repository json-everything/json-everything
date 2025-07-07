using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

// Output is a tree structure that mostly follows the structure
// of the schema, but expands as cycles are encountered.
// Each node represents a subschema validation.
public sealed class EvaluationOutput(
    bool isValid,
    JsonPointer instanceLocation,
    JsonPointer evaluationPath,
    Uri schemaLocation,
    Dictionary<string, string>? errors,
    Dictionary<string, JsonElement>? annotations,
    List<EvaluationOutput>? details)
{
    /// <summary>
    /// Whether the subschema validation succeeded.
    /// </summary>
    [JsonPropertyName("valid")]
    public bool IsValid { get; } = isValid;

    /// <summary>
    /// The instance location validated.
    /// </summary>
    [JsonPropertyName("instanceLocation")]
    public JsonPointer InstanceLocation { get; } = instanceLocation;

    /// <summary>
    /// The path along the schema that was followed to reach the subschema
    /// starting from the schema's root node.  When references are
    /// followed, the appropriate reference keyword is included as a segment
    /// in the path.
    /// </summary>
    [JsonPropertyName("evaluationPath")]
    public JsonPointer EvaluationPath { get; } = evaluationPath;

    /// <summary>
    /// The canonical location of the subschema.  This is determined by
    /// its closest ancestor that defines an $id, then a json pointer
    /// fragment to navigate to the subschema.  If the pointer fragment
    /// is empty, it is omitted.
    /// </summary>
    [JsonPropertyName("schemaLocation")]
    public Uri SchemaLocation { get; } = schemaLocation;

    /// <summary>
    /// A collection of errors produced by the keywords in the local
    /// subschema, keyed by the keyword that produced the message.
    /// Errors from subschemas are not collected here.
    /// </summary>
    [JsonPropertyName("errors")]
    public Dictionary<string, string>? Errors { get; } = errors;

    /// <summary>
    /// A collection of annotation values produced by the keywords in the local
    /// subschema, keyed by the keyword that produced the message.
    /// Annotations from subschemas are not collected here.
    /// Annotations are not included if validation of the subschema fails.
    /// </summary>
    [JsonPropertyName("annotations")]
    public Dictionary<string, JsonElement>? Annotations { get; } = annotations;

    /// <summary>
    /// Result nodes from subschemas.
    /// </summary>
    [JsonPropertyName("details")]
    public List<EvaluationOutput>? Details { get; } = details;
} 