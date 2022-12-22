﻿using System.Diagnostics.CodeAnalysis;
using System;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class BinaryComparativeExpressionNode : ComparativeExpressionNode
{
	public IBinaryComparativeOperator Operator { get; }
	public ValueExpressionNode Left { get; }
	public ValueExpressionNode Right { get; }

	public BinaryComparativeExpressionNode(IBinaryComparativeOperator op, ValueExpressionNode left, ValueExpressionNode right)
	{
		Operator = op;
		Left = left;
		Right = right;
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Left.Evaluate(globalParameter, localParameter), Right.Evaluate(globalParameter, localParameter));
	}
}

internal class BinaryComparativeExpressionParser : IComparativeExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ComparativeExpressionNode? expression)
	{
		var i = index;

		// parse value
		if (!ValueExpressionParser.TryParse(source, ref i, out var left))
		{
			expression = null;
			return false;
		}

		// parse operator
		if (!BinaryComparativeOperatorParser.TryParse(source, ref i, out var op))
		{
			expression = null;
			return false;
		}

		// parse value
		if (!ValueExpressionParser.TryParse(source, ref i, out var right))
		{
			expression = null;
			return false;
		}

		expression = new BinaryComparativeExpressionNode(op, left, right);
		index = i;
		return true;
	}
}