using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace Json.Path
{
	public class ItemQueryIndex : IArrayIndexExpression, IObjectIndexExpression
	{
		private readonly Expression<Func<JsonElement, bool>> _expression;
		private readonly Func<JsonElement, bool> _compiled;

		public ItemQueryIndex(Expression<Func<JsonElement, bool>> expression)
		{
			_expression = expression;
			_compiled = expression.Compile();
		}

		IEnumerable<int> IArrayIndexExpression.GetIndices(JsonElement array)
		{
			return array.EnumerateArray().Select((e,i) => (e,i)).Where(x => _compiled(x.e)).Select(x => x.i);
		}

		IEnumerable<string> IObjectIndexExpression.GetProperties(JsonElement obj)
		{
			return obj.EnumerateObject().Where(x => _compiled(x.Value)).Select(x => x.Name);
		}

		internal static bool TryParse(ReadOnlySpan<char> span, ref int i, out IIndexExpression index)
		{
			throw new NotImplementedException();
		}
	}
}