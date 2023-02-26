namespace Json.Schema.Data;

/// <summary>
/// Declares the vocabularies of the supported drafts.
/// </summary>
public static class Vocabularies
{
	/// <summary>
	/// The data vocabulary ID.
	/// </summary>
	public const string DataId = "https://json-everything.net/vocabs-data-2022";

	/// <summary>
	/// The data vocabulary.
	/// </summary>
	public static readonly Vocabulary Data = new(DataId, typeof(DataKeyword));

	/// <summary>
	/// Registers the all components required to use the data vocabulary.
	/// </summary>
	public static void Register(VocabularyRegistry? vocabRegistry = null, SchemaRegistry? schemaRegistry = null)
	{
		vocabRegistry ??= VocabularyRegistry.Global;
		schemaRegistry ??= SchemaRegistry.Global;

		vocabRegistry.Register(Data);
		SchemaKeywordRegistry.Register<DataKeyword>();
		schemaRegistry.Register(MetaSchemas.Data);
	}
}