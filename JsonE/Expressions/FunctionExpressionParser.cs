using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text;

namespace Json.JsonE.Expressions;

internal class FunctionExpressionNode : ExpressionNode
{
	public FunctionDefinition Function { get; }
	public ExpressionNode[] Parameters { get; }

	public FunctionExpressionNode(FunctionDefinition function, IEnumerable<ExpressionNode> parameters)
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

internal class FunctionExpressionParser : IOperandExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ExpressionNode? expression)
	{
		throw new NotImplementedException();
	}

	public static bool TryParseFunction(ReadOnlySpan<char> source, ref int index, out List<ExpressionNode>? arguments, out FunctionDefinition? function)
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

		if (!FunctionRepository.TryGet(name!, out function))
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

		var parameterTypeList = function!.ParameterTypes;
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

			if (!ExpressionParser.TryParse(source, ref i, out var expr))
			{
				arguments = null;
				function = null;
				return false;
			}

			arguments.Add(expr!);

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