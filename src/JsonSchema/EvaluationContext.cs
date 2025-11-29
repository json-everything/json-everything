using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Provides contextual data for generating constraints and performing evaluations.
/// </summary>
public struct EvaluationContext
{
	/// <summary>
	/// Gets the evaluation options.
	/// </summary>
	/// <remarks>
	/// This may be different per run, so it's important this not be captured by constraints.
	/// </remarks>
	public EvaluationOptions Options { get; internal init; }
	
	/// <summary>
	/// Gets the dynamic scope.
	/// </summary>
	/// <remarks>
	/// The dynamic scope is the collection of URIs that evaluation has passed through to get
	/// to the current location.  This is important when processing references.
	/// </remarks>
	public DynamicScope Scope { get; internal init; }

	public JsonElement Instance { get; init; }

	public JsonPointer EvaluationPath { get; init; }

	public JsonPointer InstanceLocation { get; init; }

	public SchemaRegistry SchemaRegistry { get; internal init; }

	public bool RefIgnoresSiblingKeywords { get; internal init; }

	public List<KeywordEvaluation>? EvaluatedKeywords { get; internal set; }
}
