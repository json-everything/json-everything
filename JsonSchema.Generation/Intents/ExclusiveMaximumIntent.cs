namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `exclusiveMaximum` keyword.
/// </summary>
public class ExclusiveMaximumIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public decimal Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="ExclusiveMaximumIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ExclusiveMaximumIntent(decimal value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.ExclusiveMaximum(Value);
	}
}