using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Json.Path;

/// <summary>
/// A collection of nodes, generally resulting from an operation or query.
/// </summary>
[CollectionBuilder(typeof(NodeListBuilder), nameof(NodeListBuilder.Create))]
public class NodeList : IEnumerable<Node>
{
	private readonly IEnumerable<Node> _nodes;

	/// <summary>
	/// An empty nodelist.
	/// </summary>
	public static readonly NodeList Empty = new(Enumerable.Empty<Node>());

	/// <summary>Gets the number of elements in the collection.</summary>
	/// <returns>The number of elements in the collection.</returns>
	public int Count => _nodes.Count();

	/// <summary>Gets the element at the specified index in the read-only list.</summary>
	/// <param name="index">The zero-based index of the element to get.</param>
	/// <returns>The element at the specified index in the read-only list.</returns>
	public Node this[int index] => _nodes.ElementAt(index);

	/// <summary>
	/// Creates a new nodelist.
	/// </summary>
	/// <param name="nodes">The nodes.</param>
	public NodeList(IEnumerable<Node> nodes)
	{
		_nodes = nodes;
	}
	internal NodeList(ReadOnlySpan<Node> nodes)
	{
		_nodes = nodes.ToArray();
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	public IEnumerator<Node> GetEnumerator()
	{
		return _nodes.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

/// <summary>
/// Allows collection expression initialization.
/// </summary>
public static class NodeListBuilder
{
	/// <summary>
	/// Allows collection expression initialization.
	/// </summary>
	public static NodeList Create(ReadOnlySpan<Node> values) => new(values);
}