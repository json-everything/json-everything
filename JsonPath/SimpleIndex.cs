using System;
using System.Collections.Generic;
using System.Text.Json;

namespace JsonPath
{
	public readonly struct SimpleIndex : IIndexExpression
	{
		private readonly Index _index;

		public SimpleIndex(Index index)
		{
			_index = index;
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

		public IEnumerable<int> GetIndices(JsonElement array)
		{
			var length = array.GetArrayLength();
			var end = _index.IsFromEnd ? length - _index.Value : _index.Value;
			return new[] {end};
		}
	}
}