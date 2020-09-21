using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Path.QueryExpressions;

namespace Json.Path
{
	internal class ItemQueryIndex : IArrayIndexExpression, IObjectIndexExpression
	{
		private readonly QueryExpressionNode _expression;

		public ItemQueryIndex(QueryExpressionNode expression)
		{
			_expression = expression;
		}

		IEnumerable<int> IArrayIndexExpression.GetIndices(JsonElement array)
		{
			return array.EnumerateArray().Select((e, i) => (Evaluate(e), i))
				.Where(x => x.Item1)
				.Select(x => x.i);
		}

		IEnumerable<string> IObjectIndexExpression.GetProperties(JsonElement obj)
		{
			return obj.EnumerateObject().Select(p => (Evaluate(p.Value), p.Name))
				.Where(x => x.Item1)
				.Select(x => x.Name);
		}

		private bool Evaluate(JsonElement item)
		{
			if (_expression.OutputType != QueryExpressionType.Number ||
			    _expression.OutputType != QueryExpressionType.InstanceDependent)
				return false;

			var result = _expression.Evaluate(item);
			if (result.ValueKind != JsonValueKind.Number) return false;

			var index = result.GetDecimal();
			if (Math.Truncate(index) != index) return false;
			return true;
		}

		internal static bool TryParse(ReadOnlySpan<char> span, ref int i, out IIndexExpression index)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return $"?({_expression})";
		}
	}
}