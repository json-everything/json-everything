namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `unevaluatedProperties` keyword.
/// </summary>
public class UnevaluatedPropertiesIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The context that represents the inner requirements.
	/// </summary>
	public SchemaGenerationContextBase? Context { get; set; }

	/// <summary>
	/// Creates a new <see cref="UnevaluatedPropertiesIntent"/> instance.
	/// </summary>
	/// <param name="context">The context, or null to apply the false schema.</param>
	public UnevaluatedPropertiesIntent(SchemaGenerationContextBase? context = null)
	{
		Context = context;
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