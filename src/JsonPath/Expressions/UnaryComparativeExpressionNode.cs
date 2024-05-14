using System.Diagnostics.CodeAnalysis;
using System;
using System.Text;
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

	public override void BuildString(StringBuilder builder)
	{
		builder.Append(Operator);
		Value.BuildString(builder);
	}

	public override string ToString()
	{
		return $"{Operator}{Value}";
	}
}

internal class UnaryComparativeExpressionParser : IComparativeExpressionParser
{
	private static readonly PathExpressionParser _pathParser = new();

	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ComparativeExpressionNode? expression, PathParsingOptions options)
	{
		// currently only the "exists" operator is defined
		// it expects a path and has no operator

		// parse path
		if (!_pathParser.TryParse(source, ref index, out var path, options))
		{
			expression = null;
			return false;
		}

		// wrap in exists
		expression = new UnaryComparativeExpressionNode(Operators.Exists, path);
		return true;
	}
}