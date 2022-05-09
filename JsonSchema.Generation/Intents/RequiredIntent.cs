using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `required` keyword.
/// </summary>
public class RequiredIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The required property names.
	/// </summary>
	public List<string> RequiredProperties { get; }

	/// <summary>
	/// Creates a new <see cref="RequiredIntent"/> instance.
	/// </summary>
	/// <param name="requiredProperties">The required property names.</param>
	public RequiredIntent(List<string> requiredProperties)
	{
		RequiredProperties = requiredProperties;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Required(RequiredProperties);
	}
}