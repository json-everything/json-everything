using System.Text.Json.Nodes;

namespace Json.Schema;

/// <summary>
/// Represents an unknown format.
/// </summary>
public sealed class UnknownFormat : Format
{
	/// <summary>
	/// Creates a new <see cref="UnknownFormat"/> instance.
	/// </summary>
	/// <param name="key">The key.</param>
	public UnknownFormat(string key) : base(key) { }

	/// <summary>
	/// Validates an instance against a format and provides an error message.
	/// </summary>
	/// <param name="node">The node to validate.</param>
	/// <param name="errorMessage">An error message.</param>
	/// <returns>`true`.  Override to return another value.</returns>
	public override bool Validate(JsonNode? node, out string? errorMessage)
	{
		errorMessage = null;
		return true;
	}
}