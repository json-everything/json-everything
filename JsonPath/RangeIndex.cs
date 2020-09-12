using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace JsonPath
{
	public class RangeIndex : IArrayIndexExpression
	{
		private readonly Range _range;
		private readonly int _step;

		public RangeIndex(Range range, int step = 1)
		{
			_range = range;
			_step = step;
		}

		public IEnumerable<int> GetIndices(JsonElement array)
		{
			var length = array.GetArrayLength();
			var end = _range.End.IsFromEnd ? length - _range.End.Value : _range.End.Value;
			var max = Math.Min(length, end);

			var start = _range.Start.IsFromEnd ? length - _range.Start.Value : _range.Start.Value;
			var min = Math.Max(0, start);

			IEnumerable<int> all;
			if (min == max) all = new[] {min};
			else if (min < max) all = Enumerable.Range(min, max-min);
			else all = Enumerable.Range(max, min - max);

			var step = _step;
			if (step < 0)
			{
				all = all.Reverse();
				step = -step;
			}
			return all.Select((index, i) => (index, i))
				.Where(x => x.i % step == 0)
				.Select(x => x.index);
		}

		public static bool TryParse(ReadOnlySpan<char> span, ref int i, out IIndexExpression index)
		{
			Index start = Index.Start, end = Index.End;
			if (span.TryGetInt(ref i, out var v)) 
				start = new Index(Math.Abs(v), v < 0);
			if (span[i] != ':')
			{
				index = null;
				return false;
			}
			i++;
			if (span.TryGetInt(ref i, out v))
				end = new Index(Math.Abs(v), v < 0);
			if (span[i] != ':')
			{
				index = new RangeIndex(start..end);
				return true;
			}
			i++;
			if (!span.TryGetInt(ref i, out v))
			{
				index = null;
				return false;
			}
			index = new RangeIndex(start..end, v);
			return true;
		}

		public static implicit operator RangeIndex(Range range)
		{
			return new RangeIndex(range);
		}
	}
}