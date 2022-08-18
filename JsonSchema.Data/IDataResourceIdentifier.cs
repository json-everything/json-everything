using System.Text.Json.Nodes;

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
	/// <param name="value">If return is true, the value at the indicated location.</param>
	/// <returns>true if resolution is successful; false otherwise.</returns>
	bool TryResolve(ValidationContext context, out JsonNode? value);
}