using System;
using Json.Schema.Data.Keywords;

namespace Json.Schema.Data;

/// <summary>
/// Declares the vocabularies of the supported drafts.
/// </summary>
public static class Vocabulary
{
	/// <summary>
	/// The data vocabulary ID.
	/// </summary>
	public static readonly Uri DataId = new("https://docs.json-everything.net/schema/vocabs/data-2023");

	/// <summary>
	/// The data vocabulary.
	/// </summary>
	public static readonly Schema.Vocabulary Data =
		new(DataId,
			DataKeyword.Instance,
			OptionalDataKeyword.Instance);
}
