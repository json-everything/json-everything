using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Json.Path.Expressions;

internal static class FunctionExpressionParser
{
	public static bool TryParseFunction(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out List<ExpressionNode>? arguments, [NotNullWhen(true)] out IPathFunctionDefinition? function, PathParsingOptions options)
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

		if (!source.ConsumeWhitespace(ref i))
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

			// TODO: check parameter lists for appropriate argument type and parse accordingly
			// TODO: (maybe) support for function overloads.  See https://github.com/ietf-wg-jsonpath/draft-ietf-jsonpath-base/issues/430

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