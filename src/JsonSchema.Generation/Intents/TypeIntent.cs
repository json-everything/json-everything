namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `type` keyword.
/// </summary>
public class TypeIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The type.
	/// </summary>
	public SchemaValueType Type { get; set; }

	/// <summary>
	/// Creates a new <see cref="TypeIntent"/> instance.
	/// </summary>
	/// <param name="type">The type.</param>
	public TypeIntent(SchemaValueType type)
	{
		Type = type;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Type(Type);
	}
}