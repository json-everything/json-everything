using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Represents output from a keyword, which is necessarily distinct
/// from output from a subschema.  Multiple keyword outputs are
/// combined to produce a single subschema output.
/// </summary>
public sealed class KeywordResult(bool isValid, string? errorMessage, JsonElement? annotation, string? keywordOverride = null)
{
    /// <summary>
    /// A static instance indicating that validation was skipped because the keyword does not apply.
    /// </summary>
    public static readonly KeywordResult Skip = new(true, null, null);

    /// <summary>
    /// Whether the keyword validation succeeded.
    /// </summary>
    public bool IsValid { get; } = isValid;

    /// <summary>
    /// An error message if validation fails.
    /// </summary>
    public string? ErrorMessage { get; } = errorMessage;

    /// <summary>
    /// An annotation value.  May be present if validation fails.
    /// </summary>
    public JsonElement? Annotation { get; } = annotation;

    /// <summary>
    /// An optional override for the keyword that produced this result.
    /// Used when a keyword's validation failure should be reported under a different keyword.
    /// </summary>
    public string? KeywordOverride { get; } = keywordOverride;
} 