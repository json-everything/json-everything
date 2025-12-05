using System.Text.Json;

namespace Json.Schema.Data.Keywords;

/// <summary>
/// Provides an abstraction for different resource identifier types.
/// </summary>
public interface IDataResourceIdentifier
{
	/// <summary>
	/// Attempts to resolve a value from the specified JSON element.
	/// </summary>
	/// <param name="root">The root <see cref="JsonElement"/> to search for the desired value.</param>
	/// <param name="value">When this method returns, contains the resolved <see cref="JsonElement"/> if the operation succeeds; otherwise,
	/// contains the default value.</param>
	/// <returns>true if the value was successfully resolved; otherwise, false.</returns>
	bool TryResolve(JsonElement root, out JsonElement value);
}