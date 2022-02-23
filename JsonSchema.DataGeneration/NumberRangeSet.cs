using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.DataGeneration
{
	/// <summary>
	/// Managees a collection of number ranges as a single entity.
	/// </summary>
	public class NumberRangeSet
	{
		// The full range causes problems with random generation.  Dividing by 10 shouldn't be a big deal.
		internal const decimal MinRangeValue = decimal.MinValue / 10;
		internal const decimal MaxRangeValue = decimal.MaxValue / 10;

		private readonly NumberRange[] _ranges;

		/// <summary>
		/// Represent the empty set.
		/// </summary>
		public static NumberRangeSet None { get; }
		/// <summary>
		/// Represents the full range of representable values.
		/// </summary>
		/// <remarks>
		/// The full range has been limited to [decimal.MinValue / 10 .. decimal.MaxValue / 10] because
		/// the _actual_ full range causes problems with the random value generation algorithms.
		/// </remarks>
		public static NumberRangeSet Full { get; }
		/// <summary>
		/// Represents the range of 0 and all positive numbers.
		/// </summary>
		public static NumberRangeSet NonNegative { get; }

		/// <summary>
		/// Gets the ranges.
		/// </summary>
		public IReadOnlyList<NumberRange> Ranges => _ranges;

		static NumberRangeSet()
		{
			None = new NumberRangeSet(Array.Empty<NumberRange>());
			Full = new NumberRangeSet(new[] {new NumberRange(MinRangeValue, MaxRangeValue)});
			NonNegative = new NumberRangeSet(new[] {new NumberRange(0, MaxRangeValue)});
		}

		/// <summary>
		/// Creates a new set from a single range.
		/// </summary>
		public NumberRangeSet(NumberRange range)
			: this(new[] {range}) { }

		/// <summary>
		/// Copies a range set.
		/// </summary>
		public NumberRangeSet(NumberRangeSet other)
			: this(other._ranges) { }

		/// <summary>
		/// Creates a new set from a collection of ranges.
		/// </summary>
		/// <param name="other"></param>
		public NumberRangeSet(IEnumerable<NumberRange> other)
		{
			_ranges = other.ToArray();
		}

		/// <summary>
		/// Calculates the union of two sets.
		/// </summary>
		/// <returns>The resulting set of ranges that exist in either parameters.</returns>
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

		/// <summary>
		/// Calculates the union of two sets.
		/// </summary>
		/// <returns>The resulting set of ranges that exist in either parameters.</returns>
		public static NumberRangeSet Union(NumberRangeSet left, NumberRangeSet right)
		{
			return right._ranges.Aggregate(left, (current, range) => current.Union(range));
		}

		/// <summary>
		/// Calculates the set of one set omitting another.
		/// </summary>
		/// <param name="range">The operating set.</param>
		/// <returns>The resulting set of ranges that exist in this set but not the operating set.</returns>
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

		/// <summary>
		/// Calculates the set of one set omitting another.
		/// </summary>
		/// <param name="left">The source set.</param>
		/// <param name="right">The operating set.</param>
		/// <returns>The resulting set of ranges that exist in the source set but not the operating set.</returns>
		public static NumberRangeSet Subtract(NumberRangeSet left, NumberRangeSet right)
		{
			return right._ranges.Aggregate(left, (current, range) => current.Subtract(range));
		}

		/// <summary>
		/// Calculates the intersection of two sets.
		/// </summary>
		/// <returns>The resulting set of ranges that exist in both parameters.</returns>
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

		/// <summary>
		/// Calculates the intersection of two sets.
		/// </summary>
		/// <returns>The resulting set of ranges that exist in both parameters.</returns>
		public static NumberRangeSet Intersect(NumberRangeSet left, NumberRangeSet right)
		{
			var ranges = left._ranges.Join(right._ranges,
				l => true,
				r => true,
				NumberRange.Intersection)
				.SelectMany(x => x);
			return new NumberRangeSet(ranges);
			//return right._ranges.Aggregate(left, (current, range) => current.Intersect(range));
		}

		/// <summary>
		/// Gets the complement, or inversion, of the set.
		/// </summary>
		public NumberRangeSet GetComplement()
		{
			return Full - this;
		}

		/// <summary>
		/// Applies a ceiling (upper bound).
		/// </summary>
		public NumberRangeSet Ceiling(decimal ceiling)
		{
			return Intersect(new NumberRange(MinRangeValue, ceiling));
		}

		/// <summary>
		/// Applies a floor (lower bound).
		/// </summary>
		/// <param name="floor"></param>
		/// <returns></returns>
		public NumberRangeSet Floor(decimal floor)
		{
			return Intersect(new NumberRange(floor, MaxRangeValue));
		}

		private List<NumberRange> GetIntersectingRanges(NumberRange range)
		{
			return _ranges.Where(x => (x.Minimum <= range.Minimum && range.Minimum <= x.Maximum) ||
			                          (x.Minimum <= range.Maximum && range.Maximum <= x.Maximum) ||
			                          (range.Minimum <= x.Minimum && x.Maximum <= range.Maximum))
				.ToList();
		}

		/// <summary>
		/// Implicitly converts a single range to a set.
		/// </summary>
		/// <param name="range"></param>
		public static implicit operator NumberRangeSet(NumberRange range)
		{
			return new NumberRangeSet(range);
		}

		/// <summary>
		/// Unions two sets.
		/// </summary>
		public static NumberRangeSet operator +(NumberRangeSet left, NumberRangeSet right)
		{
			return Union(left, right);
		}

		/// <summary>
		/// Omits one set from another.
		/// </summary>
		public static NumberRangeSet operator -(NumberRangeSet left, NumberRangeSet right)
		{
			return Subtract(left, right);
		}

		/// <summary>
		/// Intersects two sets.
		/// </summary>
		public static NumberRangeSet operator *(NumberRangeSet left, NumberRangeSet right)
		{
			return Intersect(left, right);
		}

		/// <summary>
		/// Calculates the complement (inversion) of a set.
		/// </summary>
		/// <param name="set"></param>
		/// <returns></returns>
		public static NumberRangeSet operator !(NumberRangeSet set)
		{
			return set.GetComplement();
		}

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return string.Join(", ", _ranges);
		}
	}
}