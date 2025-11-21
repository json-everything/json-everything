using System;
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
	
	/// <summary>
	/// Gets the spec version that the schema is currently being evaluated under.
	/// </summary>
	/// <remarks>
	/// This property is informed by the `$schema` keyword and <see cref="EvaluationOptions.EvaluateAs"/>,
	/// taking `$schema` as priority.
	/// </remarks>
	public Uri EvaluatingAs { get; private set; }

	public JsonPointer EvaluationPath { get; init; }

	public JsonPointer InstanceLocation { get; init; }

	public BuildOptions BuildOptions { get; internal init; }

	public EvaluationContext(){}
}

public enum NumberProcessing
{
	Double,
	Decimal
}

public enum IntegerProcessing
{
	Int32,
	Int64,
#if NET8_0_OR_GREATER
	Int128,
#endif
	BigInt
}