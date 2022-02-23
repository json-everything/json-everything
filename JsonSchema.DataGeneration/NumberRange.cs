using System;
using System.Collections.Generic;

namespace Json.Schema.DataGeneration
{
	/// <summary>
	/// Defines a number range.
	/// </summary>
	public struct NumberRange : IEquatable<NumberRange>
	{
		/// <summary>
		/// Gets the minimum (lower bound).
		/// </summary>
		public Bound Minimum { get; }
		/// <summary>
		/// Gets the maximum (upper bound).
		/// </summary>
		public Bound Maximum { get; }

		/// <summary>
		/// Creates a new number range.
		/// </summary>
		/// <param name="minimum">The minimum</param>
		/// <param name="maximum">The maximum</param>
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

		/// <summary>
		/// Calculates the intersection of two number ranges.  May be multiple ranges.
		/// </summary>
		/// <returns>The resulting set of ranges that exist in both parameters.</returns>
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

		/// <summary>
		/// Calculates the union of two number ranges.  May be multiple ranges.
		/// </summary>
		/// <returns>The resulting set of ranges that exist in either parameters.</returns>
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

		/// <summary>
		/// Calculates the set of one range omitting another.  May be multiple ranges.
		/// </summary>
		/// <param name="a">The source range</param>
		/// <param name="b">The operating range.</param>
		/// <returns>The resulting set of ranges that exist in the source range but not the operating range.</returns>
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

		/// <summary>
		/// Calculates whether a single value is contained in the range.
		/// </summary>
		/// <returns>True if the range contains the value; false otherwise.</returns>
		public bool Contains(decimal value)
		{
			var meetsMinimum = Minimum.Inclusive ? Minimum.Value <= value : Minimum.Value < value;
			var meetsMaximum = Maximum.Inclusive ? value <= Maximum.Value : value < Maximum.Value;

			return meetsMinimum && meetsMaximum;
		}

		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>The fully qualified type name.</returns>
		public override string ToString()
		{
			var minBound = Minimum.Inclusive ? '[' : '(';
			var maxBound = Maximum.Inclusive ? ']' : ')';

			return $"{minBound}{Minimum.Value}..{Maximum.Value}{maxBound}";
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(NumberRange other)
		{
			return Minimum.Equals(other.Minimum) && Maximum.Equals(other.Maximum);
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
		public override bool Equals(object? obj)
		{
			return obj is NumberRange other && Equals(other);
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (Minimum.GetHashCode() * 397) ^ Maximum.GetHashCode();
			}
		}

		/// <summary>
		/// Compares two ranges for equality.
		/// </summary>
		/// <returns>True if the ranges are the same; false otherwise.</returns>
		public static bool operator ==(NumberRange left, NumberRange right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two ranges for non-equality.
		/// </summary>
		/// <returns>False if the ranges are the same; true otherwise.</returns>
		public static bool operator !=(NumberRange left, NumberRange right)
		{
			return !left.Equals(right);
		}
	}
}