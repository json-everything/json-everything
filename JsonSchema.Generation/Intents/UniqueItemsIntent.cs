namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `uniqueItems` keyword.
/// </summary>
public class UniqueItemsIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public bool Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="UniqueItemsIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public UniqueItemsIntent(bool value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.UniqueItems(Value);
	}
}