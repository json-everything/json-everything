using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

public class DynamicScope : IEnumerable<Uri>
{
	private readonly Stack<Uri> _scope;

	public Uri LocalScope => _scope.Peek();

	internal DynamicScope(Uri initialScope)
	{
		_scope = new Stack<Uri>();
		_scope.Push(initialScope);
	}

	private DynamicScope(IEnumerable<Uri> scope)
	{
		_scope = new Stack<Uri>(scope);
	}

	public void Push(Uri newLocal)
	{
		_scope.Push(newLocal);
	}

	public void Pop()
	{
		_scope.Pop();
	}

	public DynamicScope Append(Uri localScope)
	{
		return new DynamicScope(_scope.Append(localScope));
	}

	public IEnumerator<Uri> GetEnumerator()
	{
		return ((IEnumerable<Uri>)_scope).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}