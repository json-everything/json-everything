using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class ValueComparisonLogicalExpressionNode : LeafLogicalExpressionNode
{
	public ValueExpressionNode Left { get; }
	public IBinaryComparativeOperator Operator { get; }
	public ValueExpressionNode Right { get; }

	public ValueComparisonLogicalExpressionNode(ValueExpressionNode left, IBinaryComparativeOperator op, ValueExpressionNode right)
	{
		Left = left;
		Operator = op;
		Right = right;
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Left.Evaluate(globalParameter, localParameter), Right.Evaluate(globalParameter, localParameter));
	}

	public override void BuildString(StringBuilder builder)
	{
		Left.BuildString(builder);
		builder.Append(Operator);
		Right.BuildString(builder);
	}

	public override string ToString()
	{
		return $"{Left}{Operator}{Right}";
	}
}

internal class ValueComparisonLogicalExpressionParser : ILogicalExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, int nestLevel, [NotNullWhen(true)] out LogicalExpressionNode? expression, PathParsingOptions options)
	{
		var i = index;

		if (!source.ConsumeWhitespace(ref i))
		{
			expression = null;
			return false;
		}

		// parse value
		if (!ValueExpressionParser.TryParse(source, ref i, 0, out var left, options))
		{
			expression = null;
			return false;
		}

		// parse operator
		if (!ComparativeOperatorParser.TryParse(source, ref i, out var op))
		{
			expression = null;
			return false;
		}

		if (op is InOperator && !options.AllowInOperator)
		{
			expression = null;
			return false;
		}

		// parse value
		if (!ValueExpressionParser.TryParse(source, ref i, 0, out var right, options))
		{
			expression = null;
			return false;
		}

		if (IsNonSingularPath(left) || IsNonSingularPath(right))
		{
			expression = null;
			return false;
		}

		if (!source.ConsumeWhitespace(ref i))
		{
			expression = null;
			return false;
		}

		expression = new ValueComparisonLogicalExpressionNode(left, op, right);
		index = i;
		return true;
	}

	private static bool IsNonSingularPath(ValueExpressionNode node)
	{
		return node is PathValueExpressionNode { Path.IsSingular: false };
	}
}