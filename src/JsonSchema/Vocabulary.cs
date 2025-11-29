using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

/// <summary>
/// Represents a collection of keyword handlers associated with a specific vocabulary identifier.
/// </summary>
/// <remarks>Only JSON Schema drafts 2019-09 and 2020-12 make use of vocabularies. A vocabulary defines a set of
/// keywords and their corresponding handlers, typically used to interpret or validate structured data according
/// to a particular specification. The vocabulary is identified by a unique URI, and its keywords determine the
/// behaviors or constraints it supports.</remarks>
public partial class Vocabulary
{
	/// <summary>
	/// Gets the unique identifier for this resource as a URI.
	/// </summary>
	public Uri Id { get; }
	
	/// <summary>
	/// Gets the collection of keyword handlers associated with the current instance.
	/// </summary>
	public IKeywordHandler[] Keywords { get; }

	/// <summary>
	/// Initializes a new instance of the Vocabulary class with the specified identifier and keyword handlers.
	/// </summary>
	/// <remarks>All provided keyword handler collections are combined into a single set for the vocabulary.</remarks>
	/// <param name="id">The unique identifier for the vocabulary. Cannot be null.</param>
	/// <param name="keywords">One or more collections of keyword handlers to associate with the vocabulary.</param>
	public Vocabulary(Uri id, params IEnumerable<IKeywordHandler> keywords)
	{
		Id = id;
		Keywords = keywords.ToArray();

		if (Keywords.Length == 0)
			throw new ArgumentException("At least one keyword handler is required.");
	}
}