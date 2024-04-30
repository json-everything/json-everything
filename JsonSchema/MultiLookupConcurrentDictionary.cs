using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Json.Schema;

internal class MultiLookupConcurrentDictionary<TValue> : IEnumerable<KeyValuePair<object, TValue>>
{
	private readonly ConcurrentDictionary<object, TValue> _lookup = new();
	private readonly List<Func<TValue, object>> _keyFunctions = new();

	public int Count => _lookup.Count;

	public bool IsReadOnly => false;

	public TValue this[object key]
	{
		get => _lookup[key!];
		set => _lookup[key!] = value;
	}

	public void AddLookup(Func<TValue, object> lookup)
	{
		_keyFunctions.Add(lookup);
	}

	public void Clear() => _lookup.Clear();

	public void Add(TValue value)
	{
		foreach (var lookup in _keyFunctions)
		{
			var key = lookup(value);
			_lookup.TryAdd(key, value);
		}
	}

	public bool ContainsKey(object key) => _lookup.ContainsKey(key);

	public void Remove(TValue value)
	{
		foreach (var lookup in _keyFunctions)
		{
			var key = lookup(value);
			_lookup.TryRemove(key, out _);
		}
	}

	public bool TryGetValue(object key, [UnscopedRef] out TValue value) => _lookup.TryGetValue(key, out value);

	public TValue? GetValueOrDefault(object key) => _lookup.GetValueOrDefault(key);

	public IEnumerator<KeyValuePair<object, TValue>> GetEnumerator() => _lookup.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}