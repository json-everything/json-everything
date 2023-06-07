namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `unevaluatedProperties` keyword.
/// </summary>
public class UnevaluatedPropertiesIntent : ISchemaKeywordIntent, IContextContainer
{
	/// <summary>
	/// The context that represents the inner requirements.
	/// </summary>
	public SchemaGenerationContextBase? Context { get; private set; }

	/// <summary>
	/// Creates a new <see cref="UnevaluatedPropertiesIntent"/> instance.
	/// </summary>
	/// <param name="context">The context, or null to apply the false schema.</param>
	public UnevaluatedPropertiesIntent(SchemaGenerationContextBase? context = null)
	{
		Context = context;
	}

	/// <summary>
	/// Replaces one context with another.
	/// </summary>
	/// <param name="hashCode">The hashcode of the context to replace.</param>
	/// <param name="newContext">The new context.</param>
	public void Replace(int hashCode, SchemaGenerationContextBase newContext)
	{
		if (Context?.Hash == hashCode)
			Context = newContext;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.UnevaluatedProperties(Context?.Apply() ?? false);
	}
}