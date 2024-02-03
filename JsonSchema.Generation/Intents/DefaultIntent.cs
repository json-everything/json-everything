using System.Text.Json.Nodes;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `default` keyword.
/// </summary>
public class DefaultIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The expected value.
	/// </summary>
	public JsonNode? Value { get; }

	/// <summary>
	/// Creates a new <see cref="DefaultIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public DefaultIntent(JsonNode? value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Default(Value);
	}
}