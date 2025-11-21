namespace Json.Schema;

public class BuildOptions
{
	/// <summary>
	/// The default settings.
	/// </summary>
	public static BuildOptions Default { get; } = new();

	/// <summary>
	/// The local schema registry.  If a schema is not found here, it will
	/// automatically check the global registry as well.
	/// </summary>
	public SchemaRegistry SchemaRegistry { get; set; }

	/// <summary>
	/// Gets the registry that manages schema keywords and their associated handlers.
	/// </summary>
	/// <remarks>Use this property to access or query the set of supported keywords for schema validation and
	/// processing. The returned registry is read-only and reflects the current configuration of recognized
	/// keywords.</remarks>
	public SchemaKeywordRegistry KeywordRegistry { get; set; }

	/// <summary>
	/// Specifies whether the `format` keyword should fail validations for
	/// unknown formats.  Default is false.
	/// </summary>
	/// <remarks>
	///	This option is applied whether `format` is using annotation or
	/// assertion behavior.
	/// </remarks>
	public bool OnlyKnownFormats { get; set; }

	/// <summary>
	/// Specifies whether custom keywords that aren't defined in vocabularies
	/// should be processed.  Default is false.
	/// </summary>
	/// <remarks>
	/// Custom keywords are those which have associated <see cref="IJsonSchemaKeyword"/>
	/// implementations.  Unrecognized keywords, for which annotations should
	/// be collected, are not considered "custom."
	/// </remarks>
	public bool ProcessCustomKeywords { get; set; }

	/// <summary>
	/// Gets or sets whether `$ref` is permitted to navigate into unknown keywords
	/// where subschemas aren't expected.  Default is true.
	/// </summary>
	public bool AllowReferencesIntoUnknownKeywords { get; set; } = true;

	public BuildOptions()
		: this(SchemaRegistry.Global, SchemaKeywordRegistry.Default)
	{
	}

	private BuildOptions(SchemaRegistry schemaRegistry, SchemaKeywordRegistry keywordRegistry)
	{
		SchemaRegistry = schemaRegistry;
		KeywordRegistry = keywordRegistry;
	}
}