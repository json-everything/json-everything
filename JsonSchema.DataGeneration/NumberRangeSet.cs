using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.DataGeneration
{
	public class NumberRangeSet
	{
		// The full range causes problems with random generation.  Dividing by 10 shouldn't be a big deal.
		private const decimal _min = decimal.MinValue / 10;
		private const decimal _max = decimal.MaxValue / 10;

		private readonly NumberRange[] _ranges;

		public static NumberRangeSet None { get; }
		public static NumberRangeSet Full { get; }
		public static NumberRangeSet NonNegative { get; }

		public IReadOnlyList<NumberRange> Ranges => _ranges;

		static NumberRangeSet()
		{
			None = new NumberRangeSet(Array.Empty<NumberRange>());
			Full = new NumberRangeSet(new[] {new NumberRange(_min, _max)});
			NonNegative = new NumberRangeSet(new[] {new NumberRange(0, _max)});
		}

		public NumberRangeSet(NumberRange range)
			: this(new[] {range}) { }

		public NumberRangeSet(NumberRangeSet other)
			: this(other._ranges) { }

		public NumberRangeSet(IEnumerable<NumberRange> other)
		{
			_ranges = other.ToArray();
		}

		public NumberRangeSet Union(NumberRange range)
		{
			int index;
			var newRanges = new List<NumberRange>(_ranges);
			var intersecting = GetIntersectingRanges(range);
			if (intersecting.Count == 0)
			{
				index = newRanges.FindIndex(x => range.Minimum < x.Minimum);
				newRanges.Insert(index, range);
			}
			else
			{
				var first = intersecting.First();
				var last = intersecting.Last();
				var newMinimum = Bound.Minimum(first.Minimum, range.Minimum);
				var newMaximum = Bound.Maximum(last.Maximum, range.Maximum);
				var overarchingRange = new NumberRange(newMinimum, newMaximum);
				index = newRanges.IndexOf(first);
				newRanges.RemoveRange(index, intersecting.Count);
				newRanges.Insert(index, overarchingRange);
			}

			return new NumberRangeSet(newRanges);
		}

		public static NumberRangeSet Union(NumberRangeSet left, NumberRangeSet right)
		{
			return right._ranges.Aggregate(left, (current, range) => current.Union(range));
		}

		public NumberRangeSet Subtract(NumberRange range)
		{
			var intersecting = GetIntersectingRanges(range);
			if (intersecting.Count == 0)
				return new NumberRangeSet(_ranges);

			var first = intersecting.First();
			var last = intersecting.Last();
			var newMinimum = Bound.Minimum(first.Minimum, range.Minimum);
			var newMaximum = Bound.Maximum(last.Maximum, range.Maximum);
			var overarchingRange = new NumberRange(newMinimum, newMaximum);
			var difference = NumberRange.Difference(overarchingRange, range);
			var newRanges = new List<NumberRange>(_ranges);
			var index = newRanges.IndexOf(first);
			newRanges.RemoveRange(index, intersecting.Count);
			newRanges.InsertRange(index, difference);
			return new NumberRangeSet(newRanges);
		}

		public static NumberRangeSet Subtract(NumberRangeSet left, NumberRangeSet right)
		{
			return right._ranges.Aggregate(left, (current, range) => current.Subtract(range));
		}

		private NumberRangeSet Intersect(NumberRange range)
		{
			var intersecting = GetIntersectingRanges(range);
			if (intersecting.Count == 0)
				return new NumberRangeSet(Array.Empty<NumberRange>());

			var first = intersecting.First();
			var last = intersecting.Last();
			var newRanges = new List<NumberRange>(_ranges);
			var newMinimum = Bound.Maximum(first.Minimum, range.Minimum);
			var newMaximum = Bound.Minimum(last.Maximum, range.Maximum);

			if (first == last)
				return new NumberRange(newMinimum, newMaximum);
			
			var newFirst = new NumberRange(newMinimum, first.Maximum);
			var index = newRanges.IndexOf(first);
			newRanges[index] = newFirst;
			var newLast = new NumberRange(last.Minimum, newMaximum);
			index = newRanges.IndexOf(last);
			newRanges[index] = newLast;

			return new NumberRangeSet(newRanges);
		}

		public static NumberRangeSet Intersect(NumberRangeSet left, NumberRangeSet right)
		{
			return right._ranges.Aggregate(left, (current, range) => current.Intersect(range));
		}

		public NumberRangeSet Invert()
		{
			return Full - this;
		}

		public NumberRangeSet Ceiling(decimal ceiling)
		{
			return Intersect(new NumberRange(_min, ceiling));
		}

		public NumberRangeSet Floor(decimal floor)
		{
			return Intersect(new NumberRange(floor, _max));
		}

		private List<NumberRange> GetIntersectingRanges(NumberRange range)
		{
			return _ranges.Where(x => (x.Minimum <= range.Minimum && range.Minimum <= x.Maximum) ||
			                          (x.Minimum <= range.Maximum && range.Maximum <= x.Maximum) ||
			                          (range.Minimum <= x.Minimum && x.Maximum <= range.Maximum))
				.ToList();
		}

		public static implicit operator NumberRangeSet(NumberRange range)
		{
			return new NumberRangeSet(range);
		}

		public static NumberRangeSet operator +(NumberRangeSet left, NumberRangeSet right)
		{
			return Union(left, right);
		}

		public static NumberRangeSet operator -(NumberRangeSet left, NumberRangeSet right)
		{
			return Subtract(left, right);
		}

		public static NumberRangeSet operator *(NumberRangeSet left, NumberRangeSet right)
		{
			return Intersect(left, right);
		}

		public static NumberRangeSet operator !(NumberRangeSet set)
		{
			return set.Invert();
		}

		public override string ToString()
		{
			return string.Join(", ", _ranges);
		}
	}
}