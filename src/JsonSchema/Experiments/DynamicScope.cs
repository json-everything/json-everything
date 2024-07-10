using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Experiments;

public class DynamicScope : IEnumerable<Uri>, IEquatable<DynamicScope>
{
	private readonly Stack<Uri> _scope;

	/// <summary>
	/// Gets the local scope, or the most recent schema URI encountered.
	/// </summary>
	public Uri LocalScope => _scope.Count == 0 ? null! : _scope.Peek();

	internal DynamicScope()
	{
		_scope = new Stack<Uri>();
	}

	internal void Push(Uri newLocal)
	{
		_scope.Push(newLocal);
	}

	internal void Pop()
	{
		_scope.Pop();
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	public IEnumerator<Uri> GetEnumerator()
	{
		return ((IEnumerable<Uri>)_scope).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(DynamicScope? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;

		return _scope.SequenceEqual(other._scope);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object? obj)
	{
		return Equals(obj as DynamicScope);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return _scope.GetHashCode();
	}
}