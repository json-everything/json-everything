using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema;

public class ConstraintBuilderContext
{
	// TODO: This should just be the schema registry
	public EvaluationOptions Options { get; }
	internal Stack<(string, JsonPointer)> NavigatedReferences { get; } = new();

	internal ConstraintBuilderContext(EvaluationOptions options)
	{
		Options = options;
	}
}