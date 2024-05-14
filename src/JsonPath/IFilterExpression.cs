using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Exposes the filter expression.
/// </summary>
public interface IFilterExpression
{
	/// <summary>
	/// Evaluates the selector.
	/// </summary>
	/// <param name="globalParameter">The root node of the data, represented by `$`.</param>
	/// <param name="localParameter">The current node in the filter, represented by `@`.</param>
	/// <returns>
	/// true if the node should be selected; false otherwise.
	/// </returns>
	bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter);
	/// <summary>
	/// Builds a string using a string builder.
	/// </summary>
	/// <param name="builder">The string builder.</param>
	void BuildString(StringBuilder builder);
}