namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `format` keyword.
/// </summary>
public class FormatIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The format.
	/// </summary>
	public Format Format { get; set; }

	/// <summary>
	/// Creates a new <see cref="FormatIntent"/> instance.
	/// </summary>
	/// <param name="format">The format.</param>
	public FormatIntent(Format format)
	{
		Format = format;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Format(Format);
	}
}