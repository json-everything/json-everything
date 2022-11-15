using System;
using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Defines basic functionality for schema keywords.
/// </summary>
public interface IJsonSchemaKeyword
{
	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	void Evaluate(EvaluationContext context);

	IEnumerable<IRequirement> GetRequirements(JsonPointer evaluationPath, Uri baseUri, JsonPointer instanceLocation);
}