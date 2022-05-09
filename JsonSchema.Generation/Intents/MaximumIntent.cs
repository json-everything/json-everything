namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `maximum` keyword.
/// </summary>
public class MaximumIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public decimal Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="MaximumIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public MaximumIntent(decimal value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Maximum(Value);
	}
}