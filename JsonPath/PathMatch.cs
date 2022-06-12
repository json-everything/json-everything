using System.Diagnostics;
using System.Text.Json.Nodes;

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
	public JsonNode? Value { get; }
	/// <summary>
	/// The location where the value was found.
	/// </summary>
	public JsonPath Location { get; }

	internal PathMatch(in JsonNode? value, in JsonPath location)
	{
		Value = value;
		Location = location;
	}
}