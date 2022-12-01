using System;
using System.Collections;
using System.Collections.Generic;

namespace Json.Schema;

/// <summary>
/// Tracks the dynamic scope during schema evaluation.
/// </summary>
/// <remarks>
/// Dynamic scope is the collection of URIs (defined by `$id`) represented by the evaluation path.
/// </remarks>
public class DynamicScope : IEnumerable<Uri>
{
	private readonly Stack<Uri> _scope;

	/// <summary>
	/// Gets the local scope, or the most recent schema URI encountered.
	/// </summary>
	public Uri LocalScope => _scope.Peek();

	internal DynamicScope(Uri initialScope)
	{
		_scope = new Stack<Uri>();
		_scope.Push(initialScope);
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
}