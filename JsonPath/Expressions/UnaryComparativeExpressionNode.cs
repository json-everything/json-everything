using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class UnaryComparativeExpressionNode : ComparativeExpressionNode
{
	public IUnaryComparativeOperator Operator { get; }
	public ValueExpressionNode Value { get; }

	public UnaryComparativeExpressionNode(IUnaryComparativeOperator op, ValueExpressionNode value)
	{
		Operator = op;
		Value = value;
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Value.Evaluate(globalParameter, localParameter));
	}
}

internal class UnaryComparativeExpressionParser : IComparativeExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ComparativeExpressionNode? expression)
	{
		// currently only the "exists" operator is defined
		// it expects a path and has no operator

		// parse path
		// wrap in exists

		throw new NotImplementedException();
	}
}