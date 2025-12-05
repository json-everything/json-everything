using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `allOf` keyword.
/// </summary>
public class AllOfIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// Gets the subschemas to include.
	/// </summary>
	public List<IEnumerable<ISchemaKeywordIntent>> Subschemas { get; }

	/// <summary>
	/// Creates a new instance of the <see cref="AllOfIntent"/> class.
	/// </summary>
	/// <param name="subschemas">The subschemas to include.</param>
	public AllOfIntent(IEnumerable<IEnumerable<ISchemaKeywordIntent>> subschemas)
	{
		Subschemas = subschemas.ToList();
	}

	/// <summary>
	/// Creates a new instance of the <see cref="AllOfIntent"/> class.
	/// </summary>
	/// <param name="subschemas">The subschemas to include.</param>
	public AllOfIntent(params IEnumerable<ISchemaKeywordIntent>[] subschemas)
	{
		Subschemas = [.. subschemas];
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.AllOf(Subschemas.Select(Build));
	}

	private static JsonSchemaBuilder Build(IEnumerable<ISchemaKeywordIntent> subschema)
	{
		var builder = new JsonSchemaBuilder();

		foreach (var intent in subschema)
		{
			intent.Apply(builder);
		}

		return builder;
	}
}