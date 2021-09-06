using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Json.Path.QueryExpressions;

namespace Json.Path
{
	internal class ContainerQueryIndex : IArrayIndexExpression, IObjectIndexExpression
	{
		private readonly QueryExpressionNode _expression;

		private ContainerQueryIndex(QueryExpressionNode expression)
		{
			_expression = expression;
		}

		IEnumerable<int> IArrayIndexExpression.GetIndices(JsonElement array)
		{
			if (_expression.OutputType != QueryExpressionType.Number &&
			    _expression.OutputType != QueryExpressionType.InstanceDependent)
				return new int[] { };

			var result = _expression.Evaluate(array);
			if (result.ValueKind != JsonValueKind.Number) return new int[] { };

			var index = result.GetDecimal();
			if (Math.Truncate(index) != index) return new int[] { };
			return new[] {(int) index};
		}

		IEnumerable<string> IObjectIndexExpression.GetProperties(JsonElement obj)
		{
			if (_expression.OutputType != QueryExpressionType.String &&
			    _expression.OutputType != QueryExpressionType.InstanceDependent)
				return new string[] { };

			var result = _expression.Evaluate(obj);
			if (result.ValueKind != JsonValueKind.String) return new string[] { };

			var index = result.GetString();
			return new[] {index};
		}

		internal static bool TryParse(ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out IIndexExpression? index)
		{
			if (span[i] != '(')
			{
				i = -1;
				index = null;
				return false;
			}

			var localIndex = i;
			if (!span.TryParseExpression(ref localIndex, out var expression) ||
			    !(expression.OutputType == QueryExpressionType.Number || 
			      expression.OutputType == QueryExpressionType.InstanceDependent))
			{
				index = null;
				return false;
			}

			i = localIndex;
			if (i >= span.Length)
			{
				index = null;
				return false;
			}

			index = new ContainerQueryIndex(expression);
			return true;
		}

		public override string ToString()
		{
			return $"({_expression})";
		}
	}
} 