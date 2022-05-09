namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `title` keyword.
/// </summary>
public class TitleIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="TitleIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public TitleIntent(string value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Title(Value);
	}
}