namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `readOnly` keyword.
/// </summary>
public class ReadOnlyIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public bool Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="ReadOnlyIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ReadOnlyIntent(bool value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.ReadOnly(Value);
	}
}