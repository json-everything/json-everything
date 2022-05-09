namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `multipleOf` keyword.
/// </summary>
public class MultipleOfIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public decimal Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="MultipleOfIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public MultipleOfIntent(decimal value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.MultipleOf(Value);
	}
}