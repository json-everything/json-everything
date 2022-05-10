using System.Collections.Generic;
using System.Linq;
using Json.Pointer;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `anyOf` keyword.
/// </summary>
public class AnyOfIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// Gets the subschemas to include.
	/// </summary>
	public List<IEnumerable<ISchemaKeywordIntent>> Subschemas { get; }

	/// <summary>
	/// Creates a new instance of the <see cref="AnyOfIntent"/> class.
	/// </summary>
	/// <param name="subschemas">The subschemas to include.</param>
	public AnyOfIntent(IEnumerable<IEnumerable<ISchemaKeywordIntent>> subschemas)
	{
		Subschemas = subschemas.ToList();
	}

	/// <summary>
	/// Creates a new instance of the <see cref="AnyOfIntent"/> class.
	/// </summary>
	/// <param name="subschemas">The subschemas to include.</param>
	public AnyOfIntent(params IEnumerable<ISchemaKeywordIntent>[] subschemas)
	{
		Subschemas = subschemas.ToList();
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.AnyOf(Subschemas.Select(Build));
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