using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Json.Path
{
	internal class SliceIndex : IArrayIndexExpression
	{
		private readonly Range _range;
		private readonly int _step;

		private SliceIndex(Range range, int step = 1)
		{
			_range = range;
			_step = step;
		}

		IEnumerable<int> IArrayIndexExpression.GetIndices(JsonElement array)
		{
			if (_step == 0) return Enumerable.Empty<int>();

			var length = array.GetArrayLength();
			var startUnspecified = _range.Start.IsFromEnd && _range.Start.Value == 0;
			int start;
			if (startUnspecified)
				start = _step < 0 ? length - 1 : 0;
			else
				start = _range.Start.IsFromEnd ? length - _range.Start.Value : _range.Start.Value;

			var endUnspecified = _range.End.IsFromEnd && _range.End.Value == 0;
			int end;
			if (endUnspecified)
				end = _step < 0 ? 0 : length;
			else
				end = _range.End.IsFromEnd ? length - _range.End.Value : _range.End.Value;

			var low = start < end ? start : end;
			low = Math.Max(0, low);
			var high = start < end ? end : start;
			high = Math.Min(length - 1, high);

			if (low > high) return Enumerable.Empty<int>();

			var indices = new List<int>();
			var current = start;

			var stepsToLow = Math.Abs((start - low) / _step);
			var stepsToHigh = Math.Abs((start - high) / _step);
			var stepsToTake = Math.Min(stepsToLow, stepsToHigh);
			current += stepsToTake * _step;

			while (low <= current && current <= high)
			{
				if (current != end || endUnspecified)
					indices.Add(current);
				current += _step;
			}

			return indices;
		}

		internal static bool TryParse(ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out IIndexExpression? index)
		{
			Index start = Index.End, end = Index.End;
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
				index = new SliceIndex(start..end);
				return true;
			}
			i++;
			if (!span.TryGetInt(ref i, out v))
			{
				index = new SliceIndex(start..end);
				return true;
			}
			index = new SliceIndex(start..end, v);
			return true;
		}

		public static implicit operator SliceIndex(Range range)
		{
			return new SliceIndex(range);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			if (!_range.Start.Equals(Index.End))
				sb.Append(_range.Start.ToPathString());
			sb.Append(":");
			if (!_range.End.Equals(Index.End))
				sb.Append(_range.End.ToPathString());
			if (_step != 1)
			{
				sb.Append(":");
				sb.Append(_step);
			}

			return sb.ToString();
		}
	}
}