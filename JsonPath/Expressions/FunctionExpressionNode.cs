using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class FunctionExpressionNode : ValueExpressionNode
{
	public string Name { get; }
	public IEnumerable<ValueExpressionNode> Parameters { get; }

	public override JsonNode? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		throw new NotImplementedException();
	}
}