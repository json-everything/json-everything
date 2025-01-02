namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `additionalProperties` keyword.
/// </summary>
public class AdditionalPropertiesIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The context that represents the inner requirements.
	/// </summary>
	public SchemaGenerationContextBase Context { get; set; }

	/// <summary>
	/// Creates a new <see cref="AdditionalPropertiesIntent"/> instance.
	/// </summary>
	/// <param name="context">The context.</param>
	public AdditionalPropertiesIntent(SchemaGenerationContextBase context)
	{
		Context = context;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.AdditionalProperties(Context.Apply());
	}
}