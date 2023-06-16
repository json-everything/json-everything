using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Json.Schema.Data;

/// <summary>
/// Provides an abstraction for different resource identifier types.
/// </summary>
public interface IDataResourceIdentifier
{
	/// <summary>
	/// Attempts to resolve the reference.
	/// </summary>
	/// <param name="context">The schema evaluation context.</param>
	/// <returns>true and the node if resolution is successful; false otherwise.</returns>
	Task<(bool, JsonNode?)> TryResolve(EvaluationContext context);
}