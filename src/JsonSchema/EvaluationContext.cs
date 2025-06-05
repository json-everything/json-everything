using System;
using System.Collections.Generic;
using System.Linq;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Provides contextual data for generating constraints and performing evaluations.
/// </summary>
public class EvaluationContext
{
	private readonly Stack<SpecVersion> _evaluatingAs = new();
#if DEBUG
	private JsonPointer_Old _evaluationPath = JsonPointer_Old.Empty;
#endif

	/// <summary>
	/// Gets the evaluation options.
	/// </summary>
	/// <remarks>
	/// This may be different per run, so it's important this not be captured by constraints.
	/// </remarks>
	public EvaluationOptions Options { get; }
	
	/// <summary>
	/// Gets the dynamic scope.
	/// </summary>
	/// <remarks>
	/// The dynamic scope is the collection of URIs that evaluation has passed through to get
	/// to the current location.  This is important when processing references.
	/// </remarks>
	public DynamicScope Scope { get; }
	
	/// <summary>
	/// Gets the spec version that the schema is currently being evaluated under.
	/// </summary>
	/// <remarks>
	/// This property is informed by the `$schema` keyword and <see cref="EvaluationOptions.EvaluateAs"/>,
	/// taking `$schema` as priority.
	/// </remarks>
	public SpecVersion EvaluatingAs { get; private set; }

	internal Stack<(string, JsonPointer_Old)> NavigatedReferences { get; } = new();

	internal EvaluationContext(EvaluationOptions options, SpecVersion evaluatingAs, Uri initialScope)
	{
		Options = options;
		PushEvaluatingAs(evaluatingAs);
		Scope = new DynamicScope(initialScope);
	}

	internal void PushEvaluatingAs(SpecVersion version)
	{
		_evaluatingAs.Push(version);
		EvaluatingAs = version;
	}

	internal void PopEvaluatingAs()
	{ 
		_evaluatingAs.Pop();
		EvaluatingAs = _evaluatingAs.Peek();
	}

	internal void PushEvaluationPath(PointerSegment segment)
	{
#if DEBUG
		_evaluationPath = _evaluationPath.Combine(segment);
#endif
	}

	internal void PopEvaluationPath()
	{
#if DEBUG
		_evaluationPath = _evaluationPath.GetAncestor(1);
#endif
	}
}