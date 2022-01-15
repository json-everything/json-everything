using System;
using System.Collections.Generic;

namespace Json.Schema.DataGeneration
{
	public readonly struct Bound : IComparable<Bound>, IEquatable<Bound>
	{
		public decimal Value { get; }
		public bool Inclusive { get; }

		public Bound(decimal value, bool inclusive = true)
		{
			Value = value;
			Inclusive = inclusive;
		}

		public static implicit operator Bound(int value)
		{
			return new Bound(value);
		}

		public static implicit operator Bound(decimal value)
		{
			return new Bound(value);
		}

		public static implicit operator Bound((int value, bool inclusive) pair)
		{
			return new Bound(pair.value, pair.inclusive);
		}

		public static implicit operator Bound((decimal value, bool inclusive) pair)
		{
			return new Bound(pair.value, pair.inclusive);
		}

		public int CompareTo(Bound other)
		{
			if (Value < other.Value) return -1;
			if (Value > other.Value) return 1;

			if (Inclusive == other.Inclusive) return 0;
			if (Inclusive) return -1;
			return 1;
		}

		public override bool Equals(object obj)
		{
			return Equals((Bound) obj);
		}

		public bool Equals(Bound other)
		{
			return Value == other.Value && Inclusive == other.Inclusive;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Value.GetHashCode() * 397) ^ Inclusive.GetHashCode();
			}
		}

		public static bool operator ==(Bound left, Bound right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Bound left, Bound right)
		{
			return !left.Equals(right);
		}

		public static bool operator <(Bound left, Bound right)
		{
			return left.CompareTo(right) == -1;
		}

		public static bool operator <=(Bound left, Bound right)
		{
			return left.CompareTo(right) is -1 or 0;
		}

		public static bool operator >(Bound left, Bound right)
		{
			return left.CompareTo(right) == 1;
		}

		public static bool operator >=(Bound left, Bound right)
		{
			return left.CompareTo(right) is 1 or 0;
		}

		public static Bound Minimum(Bound a, Bound b)
		{
			return a.CompareTo(b) == 1 ? b : a;
		}

		public static Bound Maximum(Bound a, Bound b)
		{
			return a.CompareTo(b) == -1 ? b : a;
		}
	}

	public sealed class BoundEqualityComparer : IEqualityComparer<Bound>
	{
		public static IEqualityComparer<Bound> Instance { get; } = new BoundEqualityComparer();

		public bool Equals(Bound x, Bound y)
		{
			return x.Value == y.Value && x.Inclusive == y.Inclusive;
		}

		public int GetHashCode(Bound obj)
		{
			unchecked
			{
				return (obj.Value.GetHashCode() * 397) ^ obj.Inclusive.GetHashCode();
			}
		}
	}
}