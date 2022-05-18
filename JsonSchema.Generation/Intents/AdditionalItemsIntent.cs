using System.Collections.Generic;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `additionalItems` keyword.
/// </summary>
public class AdditionalItemsIntent : ISchemaKeywordIntent, IContextContainer
{
	/// <summary>
	/// The context that represents the inner requirements.
	/// </summary>
	public SchemaGenerationContextBase Context { get; private set; }

	/// <summary>
	/// Creates a new <see cref="AdditionalItemsIntent"/> instance.
	/// </summary>
	/// <param name="context">The context.</param>
	public AdditionalItemsIntent(SchemaGenerationContextBase context)
	{
		Context = context;
	}

	/// <summary>
	/// Gets the contexts.
	/// </summary>
	/// <returns>
	///	The <see cref="SchemaGenerationContextBase"/>s contained by this object.
	/// </returns>
	public IEnumerable<SchemaGenerationContextBase> GetContexts()
	{
		return new[] { Context };
	}

	/// <summary>
	/// Replaces one context with another.
	/// </summary>
	/// <param name="hashCode">The hashcode of the context to replace.</param>
	/// <param name="newContext">The new context.</param>
	public void Replace(int hashCode, SchemaGenerationContextBase newContext)
	{
		if (Context.Hash == hashCode)
			Context = newContext;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.AdditionalItems(Context.Apply());
	}
}