namespace Json.Schema.Data;

/// <summary>
/// Declares the vocabularies of the supported drafts.
/// </summary>
public static class Vocabularies
{
	/// <summary>
	/// The data vocabulary ID.
	/// </summary>
	public const string DataId = "https://docs.json-everything.net/schema/vocabs/data-2023";

	/// <summary>
	/// The data vocabulary.
	/// </summary>
	public static readonly Vocabulary Data = new(DataId, typeof(DataKeyword), typeof(OptionalDataKeyword));

	/// <summary>
	/// Registers the all components required to use the data vocabulary.
	/// </summary>
	public static void Register(VocabularyRegistry? vocabRegistry = null, SchemaRegistry? schemaRegistry = null)
	{
		vocabRegistry ??= VocabularyRegistry.Global;
		schemaRegistry ??= SchemaRegistry.Global;

		vocabRegistry.Register(Data);
		SchemaKeywordRegistry.Register<DataKeyword>(JsonSchemaDataSerializerContext.Default);
		schemaRegistry.Register(MetaSchemas.Data);
		schemaRegistry.Register(MetaSchemas.Data_202012);
	}
}
