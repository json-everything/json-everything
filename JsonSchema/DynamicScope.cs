using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

public class DynamicScope : IEnumerable<Uri>
{
	private readonly Uri[] _scope;

	public Uri LocalScope => _scope[_scope.Length - 1];

	internal DynamicScope(Uri initialScope)
	{
		_scope = new[] { initialScope };
	}

	private DynamicScope(IEnumerable<Uri> scope)
	{
		_scope = scope.ToArray();
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