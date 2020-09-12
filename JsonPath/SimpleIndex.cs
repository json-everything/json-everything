using System;
using System.Collections.Generic;
using System.Text.Json;

namespace JsonPath
{
	public class SimpleIndex : IArrayIndexExpression
	{
		private readonly Index _index;

		public SimpleIndex(Index index)
		{
			_index = index;
		}

		public IEnumerable<int> GetIndices(JsonElement array)
		{
			var length = array.GetArrayLength();
			var end = _index.IsFromEnd ? length - _index.Value : _index.Value;
			return new[] {end};
		}

		public static bool TryParse(ReadOnlySpan<char> span, ref int i, out IIndexExpression index)
		{
			if (!span.TryGetInt(ref i, out var value))
			{
				index = null;
				return false;
			}

			index = value < 0 ? new SimpleIndex(^value) : new SimpleIndex(value);
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
	}
}