using System;

namespace Json.Schema.Generation.Serialization;

/// <summary>
/// Apply to a type to generate a schema for validation during deserialization
/// by <see cref="GenerativeValidatingJsonConverter"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
public class GenerateJsonSchemaAttribute : Attribute;