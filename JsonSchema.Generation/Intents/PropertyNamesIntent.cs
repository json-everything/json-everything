using System.Collections.Generic;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `propertyNames` keyword.
/// </summary>
public class PropertyNamesIntent : ISchemaKeywordIntent, IContextContainer
{
	/// <summary>
	/// The context that represents the inner requirements.
	/// </summary>
	public SchemaGenerationContextBase Context { get; private set; }

	/// <summary>
	/// Creates a new <see cref="PropertyNamesIntent"/> instance.
	/// </summary>
	/// <param name="context">The context.</param>
	public PropertyNamesIntent(SchemaGenerationContextBase context)
	{
		Context = context;
	}

	/// <summary>
	/// Gets the contexts.
	/// </summary>
	/// <returns>
	///	The <see cref="SchemaGenerationContextBase"/>s contained by this object.
	/// </returns>
	/// <remarks>
	/// Only return the contexts contained directly by this object.  Do not fetch
	/// the child contexts of those contexts.
	/// </remarks>
	public IEnumerable<SchemaGenerationContextBase> GetContexts()
	{
		return new[] { Context };
	}

	/// <summary>
	/// Replaces one context with another.
	/// </summary>
	/// <param name="hashCode">The hashcode of the context to replace.</param>
	/// <param name="newContext">The new context.</param>
	/// <remarks>
	/// To implement this, call <see cref="object.GetHashCode()"/> on the contained
	/// contexts.  If any match, replace them with <paramref name="newContext"/>.
	/// </remarks>
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
		builder.PropertyNames(Context.Apply());
	}
}