using System;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `$id` keyword.
/// </summary>
public class IdIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The URI to use as the schema's ID.
	/// </summary>
	public Uri Id { get; }

	/// <summary>
	/// Creates a new <see cref="IdIntent"/> instance.
	/// </summary>
	public IdIntent(Uri uri)
	{
		Id = uri;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Id(Id);
	}
}