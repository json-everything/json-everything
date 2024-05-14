using System.Globalization;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Implements the `length()` function to get:
/// - the length of a string
/// - the count of values in an array
/// - the count of values in an object
/// </summary>
public class LengthFunction : ValueFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	public override string Name => "length";

	/// <summary>
	/// Evaluates the function.
	/// </summary>
	/// <param name="value">An object, array, or string</param>
	/// <returns>If an object or array, the number of items it contains; if a string, the length.</returns>
	public JsonNode? Evaluate(JsonNode? value)
	{
		return value switch
		{
			JsonObject obj => (JsonValue)obj.Count,
			JsonArray arr => (JsonValue)arr.Count,
			JsonValue val when val.TryGetValue(out string? s) => (JsonValue)new StringInfo(s).LengthInTextElements,
			_ => Nothing
		};
	}
}