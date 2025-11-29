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

	/// <summary>
	/// Specifies whether the `format` keyword should fail validations for
	/// unknown formats.  Default is false.
	/// </summary>
	/// <remarks>
	///	This option is applied whether `format` is using annotation or
	/// assertion behavior.
	/// </remarks>
	public bool OnlyKnownFormats { get; set; }
}