using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `additionalProperties` keyword.
/// </summary>
public class PropertiesIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The contexts that represent the properties.
	/// </summary>
	public Dictionary<string, SchemaGenerationContextBase> Properties { get; }

	/// <summary>
	/// Creates a new <see cref="PropertiesIntent"/> instance.
	/// </summary>
	/// <param name="properties">The contexts.</param>
	public PropertiesIntent(Dictionary<string, SchemaGenerationContextBase> properties)
	{
		Properties = properties;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Properties(Properties.ToDictionary(p => p.Key, p => p.Value.Apply()));
	}
}