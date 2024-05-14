using System;

namespace Json.Schema;

/// <summary>
/// Defines basic functionality for schema keywords.
/// </summary>
public interface IJsonSchemaKeyword
{
	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	///     The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	///     Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		ReadOnlySpan<KeywordConstraint> localConstraints,
		EvaluationContext context);
}
