using System.Collections.Generic;

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

	KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context);
}
