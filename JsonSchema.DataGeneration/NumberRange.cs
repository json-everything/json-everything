using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.DataGeneration
{
	public struct NumberRange : IEquatable<NumberRange>
	{
		public Bound Minimum { get; }
		public Bound Maximum { get; }

		public NumberRange(Bound minimum, Bound maximum)
		{
			if (minimum > maximum)
			{
				Minimum = maximum;
				Maximum = minimum;
			}
			else
			{
				Minimum = minimum;
				Maximum = maximum;
			}
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

		public static IEnumerable<NumberRange> Difference(NumberRange a, NumberRange b)
		{
			if (b.Minimum < a.Minimum)
				return Difference(b, a);

			// disjoint: a1  a2  b1  b2 -> a1  a2
			if (a.Maximum < b.Minimum)
				return new[] {a};

			// contained (different end): a1  b1  b2  a2  -> a1  !b1 | !b2  a2
			// contained (same end): a1  b1  b2a2  -> a1  !b1 | !b2  a2
			if (a.Minimum < b.Minimum && b.Maximum < a.Maximum)
				return new[]
				{
					new NumberRange(a.Minimum, Bound.Complement(b.Minimum)),
					new NumberRange(Bound.Complement(b.Maximum), a.Maximum)
				};

			// intersected (different end): a1  b1  a2  b2 -> a1  !b1
			// intersected (same end): a1  b1  a2b2 -> a1  !b1
			if (a.Minimum < b.Minimum && a.Maximum < b.Maximum)
				return new[] {new NumberRange(a.Minimum, Bound.Complement(b.Minimum))};

			// same start (a ends): a1b1  b2  a2 -> !b2 a2
			if (b.Maximum < a.Maximum)
				return new[] {new NumberRange(Bound.Complement(b.Maximum), a.Maximum)};

			// same start (b ends): a1b1  a2  b2 -> a2 !b2
			if (b.Maximum < a.Maximum)
				return new[] {new NumberRange(a.Maximum, Bound.Complement(b.Maximum))};

			// perfect overlap
			return new NumberRange[] { };
		}

		public override string ToString()
		{
			var minBound = Minimum.Inclusive ? '[' : '(';
			var maxBound = Maximum.Inclusive ? ']' : ')';

			return $"{minBound}{Minimum.Value}..{Maximum.Value}{maxBound}";
		}

		public bool Equals(NumberRange other)
		{
			return Minimum.Equals(other.Minimum) && Maximum.Equals(other.Maximum);
		}

		public override bool Equals(object? obj)
		{
			return obj is NumberRange other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Minimum.GetHashCode() * 397) ^ Maximum.GetHashCode();
			}
		}

		public static bool operator ==(NumberRange left, NumberRange right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(NumberRange left, NumberRange right)
		{
			return !left.Equals(right);
		}
	}
}