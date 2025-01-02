namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `additionalItems` keyword.
/// </summary>
public class AdditionalItemsIntent : ISchemaKeywordIntent
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
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.AdditionalItems(Context.Apply());
	}
}