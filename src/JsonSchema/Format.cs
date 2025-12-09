using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Represents a format.
/// </summary>
public class Format
{
	/// <summary>
	/// The format key.
	/// </summary>
	public string Key { get; }

	/// <summary>
	/// Creates a new <see cref="Format"/>.
	/// </summary>
	/// <param name="key">The format key.</param>
	public Format(string key)
	{
		Key = key;
	}

	/// <summary>
	/// Validates an instance against a format and provides an error message.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="errorMessage">An error message.</param>
	/// <returns>`true`.  Override to return another value.</returns>
	public virtual bool Validate(JsonElement value, out string? errorMessage)
	{
		errorMessage = null;
		return true;
	}
}