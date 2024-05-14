namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `minimum` keyword.
/// </summary>
public class MinimumIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public decimal Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="MinimumIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public MinimumIntent(decimal value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Minimum(Value);
	}
}