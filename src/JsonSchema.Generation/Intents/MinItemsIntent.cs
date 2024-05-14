namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `minItems` keyword.
/// </summary>
public class MinItemsIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public uint Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="MinItemsIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public MinItemsIntent(uint value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.MinItems(Value);
	}
}