using System;
using System.Collections.Generic;

namespace Json.JsonE.Expressions;

internal static class FunctionExpressionParser
{
	public static bool TryParseFunction(ReadOnlySpan<char> source, ref int index, out List<ExpressionNode>? arguments, out IFunctionDefinition? function)
	{
		int i = index;

		if (!source.ConsumeWhitespace(ref i))
		{
			arguments = null;
			function = null;
			return false;
		}

		// parse function name
		if (!source.TryParseName(ref i, out var name))
		{
			arguments = null;
			function = null;
			return false;
		}

		if (!FunctionRepository.TryGet(name, out function))
		{
			arguments = null;
			function = null;
			return false;
		}

		if (!source.ConsumeWhitespace(ref i) || i == source.Length)
		{
			arguments = null;
			function = null;
			return false;
		}

		// consume (
		if (source[i] != '(')
		{
			arguments = null;
			function = null;
			return false;
		}

		i++;

		// parse list of arguments - all expressions
		arguments = new List<ExpressionNode>();
		var done = false;

		var parameterTypeList = ((IReflectiveFunctionDefinition)function).Evaluator.ArgTypes;
		var parameterIndex = 0;

		while (i < source.Length && !done)
		{
			if (!source.ConsumeWhitespace(ref i))
			{
				arguments = null;
				function = null;
				return false;
			}

			if (parameterIndex >= parameterTypeList.Length)
			{
				arguments = null;
				function = null;
				return false;
			}

			if (parameterTypeList[parameterIndex] == FunctionType.Value)
			{
				if (!ValueExpressionParser.TryParse(source, ref i, out var expr, options))
				{
					arguments = null;
					function = null;
					return false;
				}
				arguments.Add(expr);
			}
			else if (parameterTypeList[parameterIndex] == FunctionType.Logical)
			{

				if (!BooleanResultExpressionParser.TryParse(source, ref i, out var expr, options))
				{
					arguments = null;
					function = null;
					return false;
				}
				arguments.Add(expr);
			}
			else
			{
				// this must return a path or function that returns nodelist
				if (!ValueExpressionParser.TryParse(source, ref i, out var expr, options))
				{
					arguments = null;
					function = null;
					return false;
				}

				switch (expr)
				{
					case PathExpressionNode:
						arguments.Add(expr);
						break;
					case FunctionValueExpressionNode { Function: not NodelistFunctionDefinition }:
						arguments = null;
						function = null;
						return false;
					case FunctionValueExpressionNode funcExpr:
						arguments.Add(expr);
						break;
					default:
						arguments = null;
						function = null;
						return false;
				}
			}

			if (!source.ConsumeWhitespace(ref i))
			{
				arguments = null;
				function = null;
				return false;
			}

			switch (source[i])
			{
				case ')':
					done = true;
					break;
				case ',':
					break;
				default:
					arguments = null;
					function = null;
					return false;
			}

			i++;
			parameterIndex++;
		}

		if (parameterIndex != parameterTypeList.Length)
		{
			arguments = null;
			function = null;
			return false;
		}

		index = i;
		return true;
	}
}