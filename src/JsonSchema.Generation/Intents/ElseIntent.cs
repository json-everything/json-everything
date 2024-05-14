using System.Collections.Generic;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `else` keyword.
/// </summary>
public class ElseIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// A set of intents used to define the subschema.
	/// </summary>
	public IEnumerable<ISchemaKeywordIntent> Subschema { get; }

	/// <summary>
	/// Creates a new <see cref="ElseIntent"/> instance.
	/// </summary>
	public ElseIntent(IEnumerable<ISchemaKeywordIntent> subschema)
	{
		Subschema = subschema;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Else(Build(Subschema));
	}

	private static JsonSchema Build(IEnumerable<ISchemaKeywordIntent> subschema)
	{
		var builder = new JsonSchemaBuilder();

		foreach (var intent in subschema)
		{
			intent.Apply(builder);
		}

		return builder;
	}
}