using System;
using System.Collections.Generic;

namespace Json.Schema.DataGeneration
{
	public struct NumberRange
	{
		public Bound Minimum { get; }
		public Bound Maximum { get; }
		public bool Inverted { get; }

		public NumberRange(Bound minimum, Bound maximum, bool inverted = false)
		{
			Minimum = minimum;
			Maximum = maximum;
			Inverted = inverted;
		}

		public static IEnumerable<NumberRange> Intersection(NumberRange a, NumberRange b)
		{
			// a should have the lower bound. if not, then swap
			if (b.Minimum < a.Minimum)
				return Intersection(b, a);

			// disjoint: a1  a2  b1  b2 -> none
			if (a.Maximum < b.Minimum)
				return new NumberRange[] { };

			// tangent:  a1  a2b1  b2
			if (a.Maximum.Value == b.Minimum.Value)
			{
				// if both are inclusive -> just the single value
				if (a.Maximum.Inclusive && b.Minimum.Inclusive)
					return new[] {new NumberRange(a.Maximum, a.Maximum)};

				// otherwise disjoint
				return new NumberRange[] { };
			}

			var largestMinimum = Bound.Maximum(a.Minimum, b.Minimum);
			var smallestMaximum = Bound.Minimum(a.Maximum, b.Maximum);

			return new[] {new NumberRange(largestMinimum, smallestMaximum)};
		}

		public static IEnumerable<NumberRange> Union(NumberRange a, NumberRange b)
		{
			// a should have the lower bound. if not, then swap
			if (b.Minimum < a.Minimum)
				return Union(b, a);

			// disjoint: a1  a2  b1  b2 -> a1..a2, b1..b2
			if (a.Maximum.Value != b.Minimum.Value && a.Maximum < b.Minimum)
				return new[] {a, b};

			// tangent:  a1  a2b1  b2
			if (a.Maximum.Value == b.Minimum.Value)
			{
				// if either is inclusive -> a1..b2
				if (a.Maximum.Inclusive || b.Minimum.Inclusive)
					return new[] {new NumberRange(a.Minimum, b.Maximum)};

				// otherwise disjoint
				return new[] {a, b};
			}

			var minimum = Bound.Minimum(a.Minimum, b.Minimum);
			var maximum = Bound.Maximum(a.Maximum, b.Maximum);

			return new[] {new NumberRange(minimum, maximum)};
		}
	}
}