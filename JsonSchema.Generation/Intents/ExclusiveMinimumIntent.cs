namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `exclusiveMinimum` keyword.
/// </summary>
public class ExclusiveMinimumIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public decimal Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="ExclusiveMinimumIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ExclusiveMinimumIntent(decimal value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.ExclusiveMinimum(Value);
	}
}