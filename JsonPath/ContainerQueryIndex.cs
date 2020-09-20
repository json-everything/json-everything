using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace Json.Path
{
	public class ContainerQueryIndex : IArrayIndexExpression, IObjectIndexExpression
	{
		private readonly Expression<Func<JsonElement, IEnumerable<PathIndex>>> _expression;
		private readonly Func<JsonElement, IEnumerable<PathIndex>> _compiled;

		public ContainerQueryIndex(Expression<Func<JsonElement, IEnumerable<PathIndex>>> expression)
		{
			_expression = expression;
			_compiled = expression.Compile();
		}

		IEnumerable<int> IArrayIndexExpression.GetIndices(JsonElement array)
		{
			return _compiled(array).Where(x => x.Index.HasValue).Select(x => x.Index.Value);
		}

		IEnumerable<string> IObjectIndexExpression.GetProperties(JsonElement obj)
		{
			return _compiled(obj).Where(x => x.Name != null).Select(x => x.Name);
		}

		internal static bool TryParse(ReadOnlySpan<char> span, ref int i, out IIndexExpression index)
		{
			throw new NotImplementedException();
		}
	}
} 