using System.Diagnostics;
using System.Text.Json;

namespace Json.Path;

/// <summary>
/// Represents a single match.
/// </summary>
[DebuggerDisplay("{Value} - {Location}")]
public class PathMatch
{
	/// <summary>
	/// The value at the matching location.
	/// </summary>
	public JsonElement Value { get; }
	/// <summary>
	/// The location where the value was found.
	/// </summary>
	public JsonPath Location { get; }

	internal PathMatch(in JsonElement value, in JsonPath location)
	{
		Value = value;
		Location = location;
	}
}