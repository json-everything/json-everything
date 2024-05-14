using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Json.More;

/// <summary>
/// Provides extension functionality for <see cref="JsonDocument"/>.
/// </summary>
public static class JsonDocumentExtensions
{
	/// <summary>
	/// Determines JSON-compatible equivalence.
	/// </summary>
	/// <param name="a">The first document.</param>
	/// <param name="b">The second document.</param>
	/// <returns>`true` if the documents are equivalent; `false` otherwise.</returns>
	public static bool IsEquivalentTo(this JsonDocument a, JsonDocument b)
	{
		return a.RootElement.IsEquivalentTo(b.RootElement);
	}

	/// <summary>
	/// Converts an object to a <see cref="JsonDocument"/>.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <param name="value">The value to convert.</param>
	/// <param name="options">(optional) JSON serialization options.</param>
	/// <returns>A <see cref="JsonDocument"/> representing the vale.</returns>
	[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<T>(T, JsonSerializerOptions)")]
	[RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Serialize<T>(T, JsonSerializerOptions)")]
	public static JsonDocument ToJsonDocument<T>(this T value, JsonSerializerOptions? options = null)
	{
		if (value is JsonDocument doc) return doc;

		return JsonDocument.Parse(JsonSerializer.Serialize(value, options));
	}
}