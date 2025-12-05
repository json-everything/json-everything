using System;
using Json.Schema.ArrayExt.Keywords;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Declares the vocabularies of the supported drafts.
/// </summary>
public static class Vocabulary
{
	/// <summary>
	/// The array extensions vocabulary ID.
	/// </summary>
	public static readonly Uri ArrayExtId = new("https://docs.json-everything.net/schema/vocabs/array-ext");

	/// <summary>
	/// The array extensions vocabulary.
	/// </summary>
	public static readonly Schema.Vocabulary ArrayExt =
		new(ArrayExtId, 
			UniqueKeysKeyword.Instance,
			OrderingKeyword.Instance);
}