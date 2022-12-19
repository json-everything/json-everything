using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class PathExpressionNode : ValueExpressionNode
{
	public JsonPath Path { get; }

	public PathExpressionNode(JsonPath path)
	{
		Path = path;
	}

	public override JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		var parameter = Path.Scope == PathScope.Global
			? globalParameter
			: localParameter;

		var result = Path.Evaluate(parameter);

		return result.Matches?.Count == 1
			? result.Matches[0].Value
			: null;
	}

	public static implicit operator PathExpressionNode(JsonPath value)
	{
		return new PathExpressionNode(value);
	}
}