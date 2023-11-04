using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class FunctionValueExpressionNode : ValueExpressionNode
{
	public FunctionDefinition Function { get; }
	public ExpressionNode[] Parameters { get; }

	public FunctionValueExpressionNode(FunctionDefinition function, IEnumerable<ExpressionNode> parameters)
	{
		Function = function;
		Parameters = parameters.ToArray();
	}

	public override JsonNode? Evaluate(EvaluationContext context)
	{
		throw new NotImplementedException();
		//var parameterValues = Parameters.Select(x =>
		//{
		//	return x switch
		//	{
		//		ValueExpressionNode c => (object?)c.Evaluate(context),
		//		BooleanResultExpressionNode b => b.Evaluate(context),
		//		_ => throw new ArgumentOutOfRangeException("parameter")
		//	};
		//}).ToArray();

		//return Function.Invoke(parameterValues);
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
		throw new NotImplementedException();
		//return $"{Function.Name}({string.Join(',', Parameters.Select(x => x.ToString()).ToArray())})";
	}
}

internal class FunctionValueExpressionParser : IValueExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ValueExpressionNode? expression)
	{
		int i = index;
		if (!FunctionExpressionParser.TryParseFunction(source, ref i, out var parameters, out var function))
		{
			expression = null;
			return false;
		}

		index = i;
		expression = new FunctionValueExpressionNode(function!, parameters!);
		return true;
	}
}
