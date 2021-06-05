namespace Json.Schema.UniqueKeys
{
	/// <summary>
	/// Declares the vocabularies of the supported drafts.
	/// </summary>
	public static class Vocabularies
	{
		/// <summary>
		/// The data vocabulary ID.
		/// </summary>
		public const string UniqueKeysId = "https://gregsdennis.github.io/json-everything/vocabs-unique-keys";

		/// <summary>
		/// The data vocabulary.
		/// </summary>
		public static readonly Vocabulary UniqueKeys = new Vocabulary(UniqueKeysId, typeof(UniqueKeysKeyword));

		/// <summary>
		/// Registers the all components required to use the data vocabulary.
		/// </summary>
		public static void Register(VocabularyRegistry? vocabRegistry = null, SchemaRegistry? schemaRegistry = null)
		{
			vocabRegistry ??= VocabularyRegistry.Global;
			schemaRegistry ??= SchemaRegistry.Global;

			vocabRegistry.Register(UniqueKeys);
			SchemaKeywordRegistry.Register<UniqueKeysKeyword>();
			schemaRegistry.Register(MetaSchemas.UniqueKeysId, MetaSchemas.UniqueKeys);
		}
	}
}