namespace Json.Schema.ArrayExt;

/// <summary>
/// Declares the vocabularies of the supported drafts.
/// </summary>
public static class Vocabularies
{
	/// <summary>
	/// The data vocabulary ID.
	/// </summary>
	public const string ArrayExtId = "https://docs.json-everything.net/schema/vocabs/array-ext";

	/// <summary>
	/// The data vocabulary.
	/// </summary>
	public static readonly Vocabulary ArrayExt = new(ArrayExtId, typeof(UniqueKeysKeyword));

	/// <summary>
	/// Registers the all components required to use the data vocabulary.
	/// </summary>
	public static void Register(VocabularyRegistry? vocabRegistry = null, SchemaRegistry? schemaRegistry = null)
	{
		vocabRegistry ??= VocabularyRegistry.Global;
		schemaRegistry ??= SchemaRegistry.Global;

		vocabRegistry.Register(ArrayExt);
		SchemaKeywordRegistry.Register<UniqueKeysKeyword>();
		schemaRegistry.Register(MetaSchemas.ArrayExt_202012);
	}
}