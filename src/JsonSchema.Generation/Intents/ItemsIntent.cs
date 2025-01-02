namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `items` keyword.
/// </summary>
public class ItemsIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The context that represents the inner requirements.
	/// </summary>
	public SchemaGenerationContextBase Context { get; set; }

	/// <summary>
	/// Creates a new <see cref="ItemsIntent"/> instance.
	/// </summary>
	/// <param name="context">The context.</param>
	public ItemsIntent(SchemaGenerationContextBase context)
	{
		Context = context;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Items(Context.Apply());
	}
}