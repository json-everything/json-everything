using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Represents a subschema.
/// </summary>
public sealed class JsonSchemaNode(
    Uri baseIri,
    JsonPointer schemaLocation,
    Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
    Dictionary<string, List<JsonSchemaNode>> dependencies,
    JsonPointer schemaPathFromParent,
    JsonPointer instancePathFromParent,
    Func<JsonPointer, JsonElement, JsonElement, bool>? filterDependencyLocations)
{
    /// <summary>
    /// The path along the schema from the immediate parent of this subschema.
    /// Does not include the keyword.
    /// </summary>
    public JsonPointer AdditionalSchemaPathFromParent { get; } = schemaPathFromParent;

    /// <summary>
    /// The path along the instance from the immediate parent of this subschema.
    /// </summary>
    public JsonPointer InstancePathFromParent { get; } = instancePathFromParent;

    /// <summary>
    /// A collection of constraints to apply to the instance value.
    /// These are defined by the keywords in the local subschema.
    /// </summary>
    public Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> Constraints { get; internal set; } = constraints;

    /// <summary>
    /// A collection of subschemas defined by keywords that can hold subschemas.
    /// </summary>
    public Dictionary<string, List<JsonSchemaNode>> Dependencies { get; internal set; } = dependencies;

    /// <summary>
    /// The canonical location of the subschema.
    /// </summary>
    public Uri SchemaIri { get; } = schemaLocation.ToString() == "" ? baseIri : new Uri(baseIri, $"#{schemaLocation}");

    /// <summary>
    /// Optional filter function to determine which locations should be evaluated for dependencies.
    /// Returns true if the location should be evaluated, false to skip.
    /// </summary>
    public Func<JsonPointer, JsonElement, JsonElement, bool>? FilterDependencyLocations { get; } = filterDependencyLocations;
}
