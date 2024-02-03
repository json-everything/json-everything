using System;

namespace Json.More.Tests;

public class ObjectWithTuple : IEquatable<ObjectWithTuple>
{
	public (int, string) Tuple { get; set; }

	public bool Equals(ObjectWithTuple? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;
		return Tuple.Equals(other.Tuple);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ObjectWithTuple);
	}

	public override int GetHashCode()
	{
		// ReSharper disable once NonReadonlyMemberInGetHashCode
		return Tuple.GetHashCode();
	}
}