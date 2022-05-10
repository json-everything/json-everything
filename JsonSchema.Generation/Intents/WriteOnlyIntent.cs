namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `writeOnly` keyword.
/// </summary>
public class WriteOnlyIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public bool Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="WriteOnlyIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public WriteOnlyIntent(bool value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.WriteOnly(Value);
	}
}