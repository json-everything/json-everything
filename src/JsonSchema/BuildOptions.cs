namespace Json.Schema;

/// <summary>
/// Provides configuration options for schema building, including registries and dialect selection.
/// </summary>
/// <remarks>Use this class to customize how schemas are built, such as specifying registries for
/// schemas, vocabularies, and dialects. The options set in this class determine how schema resolution and dialect
/// selection are performed during the build process. The static <see cref="Default"/> property provides a set of
/// default options suitable for most scenarios.</remarks>
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
	public SchemaRegistry SchemaRegistry { get; set; } = SchemaRegistry.Global;

	/// <summary>
	/// The local vocabulary registry.  If a vocabulary is not found here, it will
	/// automatically check the global registry as well.
	/// </summary>
	public VocabularyRegistry VocabularyRegistry { get; set; } = VocabularyRegistry.Global;

	/// <summary>
	/// The local dialect registry.  If a dialect is not found here, it will
	/// automatically check the global registry as well.
	/// </summary>
	public DialectRegistry DialectRegistry { get; set; } = DialectRegistry.Global;

	/// <summary>
	/// Gets the dialect to be used when building schemas with these options.
	/// </summary>
	public Dialect Dialect { get; set; } = Dialect.Default;
}