using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path;

/// <summary>
/// Useful extensions for <see cref="JsonNode"/>.
/// </summary>
public static class JsonNodeExtensions
{
	/// <summary>
	/// Extends <see cref="JsonValue.TryGetValue{T}(out T)"/> to the <see cref="JsonNode"/> base class.
	/// </summary>
	/// <typeparam name="T">The type of value desired.</typeparam>
	/// <param name="node">The node that may contain the value.</param>
	/// <param name="value">The value if successful; null otherwise.</param>
	/// <returns>True if successful; false otherwise.</returns>
	public static bool TryGetValue<T>(this JsonNode? node, [NotNullWhen(true)] out T? value)
	{
		if (node is not JsonValue val)
		{
			value = default;
			return false;
		}

		return val.TryGetValue(out value);
	}

	/// <summary>
	/// Ensures a <see cref="JsonNode"/> only represents a single value.
	/// </summary>
	/// <param name="node"></param>
	/// <returns>
	/// Within the context of this library, a <see cref="NodeList"/>
	/// may be stored inside a <see cref="JsonNode"/>.  Some operations, such as
	/// expression addition, require that a single value is provided.
	///
	/// This method checks to see if the underlying value of a `JsonNode`
	/// is a `NodeList`.  If not, it simply returns the JsonNode.  If the underlying
	/// value is a `NodeList` _and_ the it only contains a single value, it
	/// returns that value.  Otherwise, it returns null.
	/// </returns>
	/// <remarks>
	/// Though a bit complex, this method can be very important for functions
	/// that require single values as inputs rather than nodelists since function
	/// composition is possible (e.g. `min(max(@,0),10)`) and functions return nodelists.
	/// </remarks>
	public static JsonNode? TryGetSingleValue(this JsonNode? node)
	{
		if (!node.TryGetValue<NodeList>(out var nodeList)) return node;

		if (nodeList.Count == 1) return nodeList[0].Value;

		return null;
	}

	/// <summary>
	/// Ensures a <see cref="NodeList"/> only represents a single value.
	/// </summary>
	/// <param name="nodeList"></param>
	/// <returns>
	/// Within the context of this library, a <see cref="NodeList"/>
	/// may be stored inside a <see cref="JsonNode"/>.  Some operations, such as
	/// expression addition, require that a single value is provided.
	///
	/// This method checks to see if the underlying value of a `JsonNode`
	/// is a `NodeList`.  If not, it simply returns the JsonNode.  If the underlying
	/// value is a `NodeList` _and_ the it only contains a single value, it
	/// returns that value.  Otherwise, it returns null.
	/// </returns>
	/// <remarks>
	/// Though a bit complex, this method can be very important for functions
	/// that require single values as inputs rather than nodelists since function
	/// composition is possible (e.g. `min(max(@,0),10)`) and functions return nodelists.
	/// </remarks>
	public static JsonNode? TryGetSingleValue(this NodeList nodeList)
	{
		return nodeList.Count == 1
			? nodeList[0].Value ?? JsonNull.SignalNode
			: null;
	}
}