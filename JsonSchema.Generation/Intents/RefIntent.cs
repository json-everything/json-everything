using System;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `$ref` keyword.
/// </summary>
public class RefIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The reference.
	/// </summary>
	public Uri Reference { get; set; }

	/// <summary>
	/// Creates a new <see cref="RefIntent"/> instance.
	/// </summary>
	/// <param name="reference">The reference.</param>
	public RefIntent(Uri reference)
	{
		Reference = reference;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Ref(Reference);
	}
}