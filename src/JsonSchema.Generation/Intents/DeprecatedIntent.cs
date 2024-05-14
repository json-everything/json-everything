namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `deprecated` keyword.
/// </summary>
public class DeprecatedIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public bool Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="DeprecatedIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DeprecatedIntent(bool value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Deprecated(Value);
	}
}