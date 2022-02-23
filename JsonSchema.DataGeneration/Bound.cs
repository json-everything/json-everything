using System;
using System.Collections.Generic;

namespace Json.Schema.DataGeneration
{
	/// <summary>
	/// Describes a lower or upper bound to a <see cref="NumberRange"/>.
	/// </summary>
	public readonly struct Bound : IComparable<Bound>, IEquatable<Bound>
	{
		/// <summary>
		/// Gets the bound value.
		/// </summary>
		public decimal Value { get; }
		/// <summary>
		/// Gets whether the value is included in the bound.
		/// </summary>
		public bool Inclusive { get; }

		/// <summary>
		/// Creates a new <see cref="Bound"/>.
		/// </summary>
		/// <param name="value">The value</param>
		/// <param name="inclusive">Whether the value is included. Default is true.</param>
		public Bound(decimal value, bool inclusive = true)
		{
			Value = value;
			Inclusive = inclusive;
		}

		/// <summary>
		/// Converts and integer to an inclusive bound.
		/// </summary>
		/// <param name="value">The value.</param>
		public static implicit operator Bound(int value)
		{
			return new Bound(value);
		}

		/// <summary>
		/// Converts a decimal to an inclusive bound.
		/// </summary>
		/// <param name="value">The value.</param>
		public static implicit operator Bound(decimal value)
		{
			return new Bound(value);
		}

		/// <summary>
		/// Converts an integer-bool tuple to a bound.
		/// </summary>
		/// <param name="pair">The value and its inclusivity.</param>
		public static implicit operator Bound((int value, bool inclusive) pair)
		{
			return new Bound(pair.value, pair.inclusive);
		}

		/// <summary>
		/// Converts a decimal-bool tuple to a bound.
		/// </summary>
		/// <param name="pair">The value and its inclusivity.</param>
		public static implicit operator Bound((decimal value, bool inclusive) pair)
		{
			return new Bound(pair.value, pair.inclusive);
		}

		/// <summary>Defines a generalized comparison method that a value type or class implements to create a type-specific comparison method for ordering or sorting its instances.</summary>
		public int CompareTo(Bound other)
		{
			if (Value < other.Value) return -1;
			if (Value > other.Value) return 1;

			if (Inclusive == other.Inclusive) return 0;
			if (Inclusive) return -1;
			return 1;
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals((Bound) obj);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(Bound other)
		{
			return Value == other.Value && Inclusive == other.Inclusive;
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (Value.GetHashCode() * 397) ^ Inclusive.GetHashCode();
			}
		}

		/// <summary>
		/// Compares two bounds for equality
		/// </summary>
		/// <returns>True if the bounds share a value and inclusivity; false otherwise.</returns>
		public static bool operator ==(Bound left, Bound right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two bounds for non-equality
		/// </summary>
		/// <returns>False if the bounds share a value and inclusivity; true otherwise.</returns>
		public static bool operator !=(Bound left, Bound right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Compares two bounds for linear order.
		/// </summary>
		/// <returns>True if left.Value &lt; right.Value or if right is inclusive and left is not.</returns>
		public static bool operator <(Bound left, Bound right)
		{
			return left.CompareTo(right) == -1;
		}

		/// <summary>
		/// Compares two bounds for linear order.
		/// </summary>
		/// <returns>True if left.Value &lt;= right.Value or if right is inclusive or both are not.</returns>
		public static bool operator <=(Bound left, Bound right)
		{
			return left.CompareTo(right) is -1 or 0;
		}

		/// <summary>
		/// Compares two bounds for linear order.
		/// </summary>
		/// <returns>True if left.Value &gt; right.Value or if left is inclusive and right is not.</returns>
		public static bool operator >(Bound left, Bound right)
		{
			return left.CompareTo(right) == 1;
		}

		/// <summary>
		/// Compares two bounds for linear order.
		/// </summary>
		/// <returns>True if left.Value &gt;= right.Value or if left is inclusive or both are not.</returns>
		public static bool operator >=(Bound left, Bound right)
		{
			return left.CompareTo(right) is 1 or 0;
		}

		/// <summary>
		/// Identifies the minimum of two bounds, including their inclusivity.
		/// </summary>
		public static Bound Minimum(Bound a, Bound b)
		{
			return a.CompareTo(b) == 1 ? b : a;
		}

		/// <summary>
		/// Identifies the maximum of two bounds, including their inclusivity.
		/// </summary>
		public static Bound Maximum(Bound a, Bound b)
		{
			return a.CompareTo(b) == -1 ? b : a;
		}

		/// <summary>
		/// Identifies the complement of a bound, which is the same value and inverted inclusivity.
		/// </summary>
		public static Bound Complement(Bound b)
		{
			return new Bound(b.Value, !b.Inclusive);
		}
	}

	/// <summary>
	/// Comparator for <see cref="Bound"/>.
	/// </summary>
	public sealed class BoundEqualityComparer : IEqualityComparer<Bound>
	{
		/// <summary>
		/// Static instance of the comparer.
		/// </summary>
		public static IEqualityComparer<Bound> Instance { get; } = new BoundEqualityComparer();

		/// <summary>Determines whether the specified objects are equal.</summary>
		/// <param name="x">The first object of type T to compare.</param>
		/// <param name="y">The second object of type T to compare.</param>
		/// <returns>true if the specified objects are equal; otherwise, false.</returns>
		public bool Equals(Bound x, Bound y)
		{
			return x.Value == y.Value && x.Inclusive == y.Inclusive;
		}

		/// <summary>Returns a hash code for the specified object.</summary>
		/// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
		/// <returns>A hash code for the specified object.</returns>
		/// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj">obj</paramref> is a reference type and <paramref name="obj">obj</paramref> is null.</exception>
		public int GetHashCode(Bound obj)
		{
			unchecked
			{
				return (obj.Value.GetHashCode() * 397) ^ obj.Inclusive.GetHashCode();
			}
		}
	}
}