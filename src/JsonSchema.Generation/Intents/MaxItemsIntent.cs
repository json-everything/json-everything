namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `maxItems` keyword.
/// </summary>
public class MaxItemsIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public uint Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="MaxItemsIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public MaxItemsIntent(uint value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.MaxItems(Value);
	}
}