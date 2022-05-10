using JetBrains.Annotations;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `pattern` keyword.
/// </summary>
public class PatternIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The value.
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="PatternIntent"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public PatternIntent([RegexPattern] string value)
	{
		Value = value;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Pattern(Value);
	}
}