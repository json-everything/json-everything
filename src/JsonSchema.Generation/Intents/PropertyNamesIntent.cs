namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `propertyNames` keyword.
/// </summary>
public class PropertyNamesIntent : ISchemaKeywordIntent
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
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.PropertyNames(Context.Apply());
	}
}