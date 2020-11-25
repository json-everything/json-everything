using System.Collections.Generic;

namespace Json.Schema.Generation
{
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
		///	The <see cref="SchemaGeneratorContext"/>s contained by this object.
		/// </returns>
		/// <remarks>
		/// Only return the contexts contained directly by this object.  Do not fetch
		/// the child contexts of those contexts.
		/// </remarks>
		IEnumerable<SchemaGeneratorContext> GetContexts();

		/// <summary>
		/// Replaces one context with another.
		/// </summary>
		/// <param name="hashCode">The hashcode of the context to replace.</param>
		/// <param name="newContext">The new context.</param>
		/// <remarks>
		/// To implement this, call <see cref="object.GetHashCode()"/> on the contained
		/// contexts.  If any match, replace them with <paramref name="newContext"/>.
		/// </remarks>
		void Replace(int hashCode, SchemaGeneratorContext newContext);
	}
}