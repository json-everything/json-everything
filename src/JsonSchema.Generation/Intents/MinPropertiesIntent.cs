namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `minProperties` keyword.
/// </summary>
public class MinPropertiesIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public uint Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="MinPropertiesIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public MinPropertiesIntent(uint value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.MinProperties(Value);
	}
}