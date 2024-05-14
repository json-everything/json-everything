namespace Json.Schema.ArrayExt;

/// <summary>
/// Declares the vocabularies of the supported drafts.
/// </summary>
public static class Vocabularies
{
	/// <summary>
	/// The array extensions vocabulary ID.
	/// </summary>
	public const string ArrayExtId = "https://docs.json-everything.net/schema/vocabs/array-ext";

	/// <summary>
	/// The array extensions vocabulary.
	/// </summary>
	public static readonly Vocabulary ArrayExt = new(ArrayExtId, typeof(UniqueKeysKeyword), typeof(OrderingKeyword));

	/// <summary>
	/// Registers the all components required to use the array extensions vocabulary.
	/// </summary>
	public static void Register(SchemaRegistry? schemaRegistry = null)
	{
		schemaRegistry ??= SchemaRegistry.Global;

		VocabularyRegistry.Register(ArrayExt);
		SchemaKeywordRegistry.Register<UniqueKeysKeyword>(JsonSchemaArrayExtSerializerContext.Default);
		SchemaKeywordRegistry.Register<OrderingKeyword>(JsonSchemaArrayExtSerializerContext.Default);
		schemaRegistry.Register(MetaSchemas.ArrayExt);
		schemaRegistry.Register(MetaSchemas.ArrayExt_202012);
	}
}
