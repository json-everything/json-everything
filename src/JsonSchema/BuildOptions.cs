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
	public Dialect KeywordRegistry { get; set; }

	/// <summary>
	/// Specifies whether the `format` keyword should fail validations for
	/// unknown formats.  Default is false.
	/// </summary>
	/// <remarks>
	///	This option is applied whether `format` is using annotation or
	/// assertion behavior.
	/// </remarks>
	public bool OnlyKnownFormats { get; set; }

	static BuildOptions()
	{
		_ = MetaSchemas.V1;
	}

	public BuildOptions()
		: this(SchemaRegistry.Global, Dialect.Default)
	{
	}

	private BuildOptions(SchemaRegistry schemaRegistry, Dialect keywordRegistry)
	{
		SchemaRegistry = schemaRegistry;
		KeywordRegistry = keywordRegistry;
	}
}