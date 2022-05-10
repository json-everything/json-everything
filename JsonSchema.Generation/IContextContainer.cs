using System;
using System.Collections.Generic;

namespace Json.Schema.Generation;

/// <summary>
/// Indicates to the generation system that this object contains contexts.
/// </summary>
/// <remarks>
/// Implement this on your <see cref="ISchemaKeywordIntent"/> to indicate that it
/// contains other contexts.  Intents that need this are generally associated with
/// applicator keywords, such as `items` and `allOf`.
/// </remarks>
public interface IContextContainer
{
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
	[Obsolete("This method is no longer used and may be implemented to simply throw an exception.")]
	IEnumerable<SchemaGenerationContextBase> GetContexts();

	/// <summary>
	/// Replaces one context with another.
	/// </summary>
	/// <param name="hashCode">The hashcode of the context to replace.</param>
	/// <param name="newContext">The new context.</param>
	/// <remarks>
	/// To implement this, call <see cref="object.GetHashCode()"/> on the contained
	/// contexts.  If any match, replace them with <paramref name="newContext"/>.
	/// </remarks>
	void Replace(int hashCode, SchemaGenerationContextBase newContext);
}