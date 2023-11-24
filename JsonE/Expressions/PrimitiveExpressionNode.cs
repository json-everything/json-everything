using System;
using System.Text;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class PrimitiveExpressionNode : ExpressionNode
{
	public JsonNode? Value { get; }

	public PrimitiveExpressionNode(JsonNode? value)
	{
		Value = value ?? JsonNull.SignalNode;
	}

	public override JsonNode? Evaluate(EvaluationContext context)
	{
		return Value;
	}

	public override void BuildString(StringBuilder builder)
	{
		builder.Append(Value.AsJsonString());
	}

	public override string ToString()
	{
		return Value.AsJsonString();
	}
}

internal class PrimitiveExpressionParser : IOperandExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ExpressionNode? expression)
	{
		if (!source.TryParsePrimitive(ref index, out var node))
		{
			expression = null;
			return false;
		}

		expression = new PrimitiveExpressionNode(node);
		return true;
	}
}