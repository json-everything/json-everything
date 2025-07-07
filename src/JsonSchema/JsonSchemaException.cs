using System;

namespace Json.Schema;

/// <summary>
/// Represents an error that occurs during JSON Schema processing.
/// </summary>
public sealed class JsonSchemaException(string message) : Exception(message); 