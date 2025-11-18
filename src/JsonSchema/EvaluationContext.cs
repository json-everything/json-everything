using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Provides contextual data for generating constraints and performing evaluations.
/// </summary>
public struct EvaluationContext
{
	private readonly Stack<Uri> _evaluatingAs = new();
#if DEBUG
	private JsonPointer _evaluationPath = JsonPointer.Empty;
#endif

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

	internal Stack<(string, JsonPointer)> NavigatedReferences { get; } = new();

	public EvaluationContext(){}

	internal void PushEvaluatingAs(Uri version)
	{
		_evaluatingAs.Push(version);
		EvaluatingAs = version;
	}

	internal void PopEvaluatingAs()
	{ 
		_evaluatingAs.Pop();
		EvaluatingAs = _evaluatingAs.Peek();
	}

	internal void PushEvaluationPath(string segment)
	{
#if DEBUG
		_evaluationPath = _evaluationPath.Combine(segment);
#endif
	}

	internal void PushEvaluationPath(int segment)
	{
#if DEBUG
		_evaluationPath = _evaluationPath.Combine(segment);
#endif
	}

	internal void PopEvaluationPath()
	{
#if DEBUG
		_evaluationPath = _evaluationPath.GetParent()!.Value;
#endif
	}
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
	Int128,
	BigInt
}