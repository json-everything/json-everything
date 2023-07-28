using System;
using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema;

public class ConstraintBuilderContext
{
	public EvaluationOptions Options { get; }
	public DynamicScope Scope { get; }

	internal SpecVersion EvaluatingAs { get; }
	internal Stack<(string, JsonPointer)> NavigatedReferences { get; } = new();

	internal ConstraintBuilderContext(EvaluationOptions options, SpecVersion evaluatingAs, Uri initialScope)
	{
		Options = options;
		EvaluatingAs = evaluatingAs;
		Scope = new DynamicScope(initialScope);
	}
}