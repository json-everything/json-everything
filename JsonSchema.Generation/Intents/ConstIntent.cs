using System.Text.Json.Nodes;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `const` keyword.
/// </summary>
public class ConstIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The expected value.
	/// </summary>
	public object? Value { get; }

	/// <summary>
	/// Creates a new <see cref="AdditionalItemsIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public ConstIntent(object? value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Const(JsonValue.Create(Value));
	}
}