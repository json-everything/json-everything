using System;
using System.Collections.Generic;

namespace Json.Schema;

/// <summary>
/// Represents a Draft 2019-09 and later vocabulary.
/// </summary>
public class Vocabulary
{
	/// <summary>
	/// The vocabulary ID.
	/// </summary>
	public Uri Id { get; }
	/// <summary>
	/// The types of the keywords that are defined by the vocabulary.
	/// </summary>
	public IReadOnlyCollection<Type> Keywords { get; }

	/// <summary>
	/// Identifies a meta-schema, if there is one for the vocabulary.
	/// </summary>
	public JsonSchema? MetaSchema { get; }

	/// <summary>
	/// Creates a new <see cref="Vocabulary"/>.
	/// </summary>
	/// <param name="id">The vocabulary ID.</param>
	/// <param name="keywords">The types of the keywords that are defined by the vocabulary.</param>
	public Vocabulary(string id, params Type[] keywords)
	{
		Id = new Uri(id, UriKind.Absolute);
		Keywords = keywords.ToReadOnlyList();
	}

	/// <summary>
	/// Creates a new <see cref="Vocabulary"/>.
	/// </summary>
	/// <param name="id">The vocabulary ID.</param>
	/// <param name="keywords">The types of the keywords that are defined by the vocabulary.</param>
	/// <param name="metaSchema">The meta-schema, if there is one for the vocabulary</param>
	public Vocabulary(string id, IEnumerable<Type> keywords, JsonSchema? metaSchema = null)
	{
		MetaSchema = metaSchema;
		Id = new Uri(id, UriKind.Absolute);
		Keywords = keywords.ToReadOnlyList();
	}
}