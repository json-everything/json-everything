using System.Threading;
using System.Threading.Tasks;

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
	/// <param name="token">A cancellation token to know if other branches of the schema have completed in an optimized evaluation.</param>
	Task Evaluate(EvaluationContext context, CancellationToken token);
}