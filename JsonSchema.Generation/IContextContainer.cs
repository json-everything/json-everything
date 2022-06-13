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