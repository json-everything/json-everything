using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class FunctionValueExpressionNode : ValueExpressionNode
{
	public IFunctionDefinition Function { get; }
	public ExpressionNode[] Parameters { get; }

	public FunctionValueExpressionNode(IFunctionDefinition function, IEnumerable<ExpressionNode> parameters)
	{
		Function = function;
		Parameters = parameters.ToArray();
	}

	public override PathValue? Evaluate(EvaluationContext context)
	{
		var parameterValues = Parameters.Select(x =>
		{
			return x switch
			{
				ValueExpressionNode c => (object?)c.Evaluate(context),
				BooleanResultExpressionNode b => b.Evaluate(globalParameter, localParameter),
				_ => throw new ArgumentOutOfRangeException("parameter")
			};
		}).ToArray();

		return Function switch
		{
			FunctionDefinition vFunc => vFunc.Invoke(parameterValues),
			NodelistFunctionDefinition nFunc => nFunc.Invoke(parameterValues),
			_ => throw new ArgumentException("This shouldn't happen.  Logical functions are not valid here.")
		};
	}

	public override void BuildString(StringBuilder builder)
	{
		builder.Append(Function.Name);
		builder.Append('(');

		if (Parameters.Any())
		{
			Parameters[0].BuildString(builder);
			for (int i = 1; i < Parameters.Length; i++)
			{
				builder.Append(',');
				Parameters[i].BuildString(builder);
			}
		}

		builder.Append(')');
	}

	public override string ToString()
	{
		return $"{Function.Name}({string.Join(',', (IEnumerable<ValueExpressionNode>)Parameters)})";
	}
}

internal class FunctionValueExpressionParser : IValueExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ValueExpressionNode? expression, PathParsingOptions options)
	{
		int i = index;
		if (!FunctionExpressionParser.TryParseFunction(source, ref i, out var parameters, out var function, options))
		{
			expression = null;
			return false;
		}

		if (function is not FunctionDefinition valueFunction)
		{
			expression = null;
			return false;
		}

		index = i;
		expression = new FunctionValueExpressionNode(valueFunction, parameters);
		return true;
	}
}
