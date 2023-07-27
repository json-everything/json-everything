using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema;

public class ConstraintBuilderContext
{
	public EvaluationOptions Options { get; }

	internal SpecVersion EvaluatingAs { get; }
	internal Stack<(string, JsonPointer)> NavigatedReferences { get; } = new();

	internal ConstraintBuilderContext(EvaluationOptions options, SpecVersion evaluatingAs)
	{
		Options = options;
		EvaluatingAs = evaluatingAs;
	}
}