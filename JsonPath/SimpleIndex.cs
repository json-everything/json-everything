using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Json.Path
{
	internal class SimpleIndex : IArrayIndexExpression
	{
		private readonly Index _index;

		private SimpleIndex(Index index)
		{
			_index = index;
		}

		IEnumerable<int> IArrayIndexExpression.GetIndices(JsonElement array)
		{
			var length = array.GetArrayLength();
			var end = _index.IsFromEnd ? length - _index.Value : _index.Value;
			return new[] {end};
		}

		internal static bool TryParse(ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out IIndexExpression? index)
		{
			if (!span.TryGetInt(ref i, out var value))
			{
				index = null;
				return false;
			}

			index = value < 0 ? new SimpleIndex(^(-value)) : new SimpleIndex(value);
			return true;
		}

		public static implicit operator SimpleIndex(Index index)
		{
			return new SimpleIndex(index);
		}

		public static implicit operator SimpleIndex(int index)
		{
			return new SimpleIndex(index);
		}

		public static implicit operator SimpleIndex(short index)
		{
			return new SimpleIndex(index);
		}

		public override string ToString()
		{
			return _index.ToPathString();
		}
	}
}