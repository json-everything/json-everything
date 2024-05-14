namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `description` keyword.
/// </summary>
public class DescriptionIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="DescriptionIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DescriptionIntent(string value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Description(Value);
	}
}