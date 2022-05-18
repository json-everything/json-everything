using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `additionalProperties` keyword.
/// </summary>
public class PropertiesIntent : ISchemaKeywordIntent, IContextContainer
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
	/// Gets the contexts.
	/// </summary>
	/// <returns>
	///	The <see cref="SchemaGenerationContextBase"/>s contained by this object.
	/// </returns>
	public IEnumerable<SchemaGenerationContextBase> GetContexts()
	{
		return Properties.Values;
	}

	/// <summary>
	/// Replaces one context with another.
	/// </summary>
	/// <param name="hashCode">The hashcode of the context to replace.</param>
	/// <param name="newContext">The new context.</param>
	public void Replace(int hashCode, SchemaGenerationContextBase newContext)
	{
		foreach (var property in Properties.ToList())
		{
			if (property.Value.Hash == hashCode)
				Properties[property.Key] = newContext;
		}
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