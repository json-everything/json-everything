namespace Json.Schema.OpenApi;

/// <summary>
/// Declares the vocabularies of the supported drafts.
/// </summary>
public static class Vocabularies
{
	/// <summary>
	/// The data vocabulary ID.
	/// </summary>
	public const string OpenApiId = "https://spec.openapis.org/oas/3.1/vocab/base";

	/// <summary>
	/// The data vocabulary.
	/// </summary>
	public static readonly Vocabulary OpenApi = new(OpenApiId,
		typeof(ExampleKeyword),
		typeof(DiscriminatorKeyword),
		typeof(ExternalDocsKeyword),
		typeof(XmlKeyword)
	);

	/// <summary>
	/// Registers the all components required to use the data vocabulary.
	/// </summary>
	public static void Register(VocabularyRegistry? vocabRegistry = null, SchemaRegistry? schemaRegistry = null)
	{
		vocabRegistry ??= VocabularyRegistry.Global;
		schemaRegistry ??= SchemaRegistry.Global;

		vocabRegistry.Register(OpenApi);
		SchemaKeywordRegistry.Register<ExampleKeyword>();
		SchemaKeywordRegistry.RegisterNullValue(new ExampleKeyword(null));
		SchemaKeywordRegistry.Register<DiscriminatorKeyword>();
		SchemaKeywordRegistry.Register<ExternalDocsKeyword>();
		SchemaKeywordRegistry.Register<XmlKeyword>();
		schemaRegistry.Register(MetaSchemas.OpenApiMeta);
	}
}
