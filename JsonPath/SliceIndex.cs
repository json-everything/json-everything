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
			var start = startUnspecified ? (int?) null : _range.Start.Value * (_range.Start.IsFromEnd ? -1 : 1);

			var endUnspecified = _range.End.IsFromEnd && _range.End.Value == 0;
			var end = endUnspecified ? (int?) null : _range.End.Value * (_range.End.IsFromEnd ? -1 : 1);

			var indices = new List<int>();
			var (lower, upper) = Bounds(start, end, _step, length);

			if (_step > 0)
			{
				var i = lower ?? 0;
				upper ??= length;
				while (i < upper)
				{
					indices.Add(i);
					i += _step;
				}
			}
			else
			{
				var i = upper ?? length - 1;
				lower ??= -1;
				while (lower < i)
				{
					indices.Add(i);
					i += _step;
				}
			}

			return indices;
		}

		private static int? Normalize(int? index, int length)
		{
			return index >= 0 ? index : length + index;
		}

		private static (int? ,int?) Bounds(int? start, int? end, int? step, int length)
		{
			var startIndex = Normalize(start, length);
			var endIndex = Normalize(end, length);

			int? lower, upper;

			if (step >= 0)
			{
				lower = startIndex.HasValue ? Math.Min(Math.Max(startIndex.Value, 0), length) : (int?) null;
				upper = endIndex.HasValue ? Math.Min(Math.Max(endIndex.Value, 0), length) : (int?) null;
			}
			else
			{
				upper = startIndex.HasValue ? Math.Min(Math.Max(startIndex.Value, -1), length-1) : (int?)null;
				lower = endIndex.HasValue ? Math.Min(Math.Max(endIndex.Value, -1), length-1) : (int?)null;
			}

			return (lower, upper);
		}

		internal static bool TryParse(ReadOnlySpan<char> span, ref int i, [NotNullWhen(true)] out IIndexExpression? index)
		{
			Index start = Index.End, end = Index.End;
			if (span.TryGetInt(ref i, out var v))
				start = new Index(Math.Abs(v), v < 0);
			if (span[i] != ':')
			{
				i = -1;
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