using System.Text.Json.Nodes;

namespace Json.Schema.Data;

/// <summary>
/// Provides an abstraction for different resource identifier types.
/// </summary>
public interface IDataResourceIdentifier
{
	/// <summary>
	/// Resolves a resource.
	/// </summary>
	/// <param name="evaluation">The evaluation being process.  This will help identify.</param>
	/// <param name="registry">The schema registry.</param>
	/// <param name="value">The value, if <paramref name="evaluation"/> was resolvable.</param>
	/// <returns>True if resolution was successful; false otherwise.</returns>
	bool TryResolve(KeywordEvaluation evaluation, SchemaRegistry registry, out JsonNode? value);
}