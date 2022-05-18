using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create a `$defs` keyword.
/// </summary>
public class DefsIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The contexts that represent the definitions.
	/// </summary>
	public Dictionary<string, SchemaGenerationContextBase> Definitions { get; }

	/// <summary>
	/// Creates a new <see cref="DefsIntent"/> instance.
	/// </summary>
	/// <param name="definitions">The contexts.</param>
	public DefsIntent(Dictionary<string, SchemaGenerationContextBase> definitions)
	{
		Definitions = definitions;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Defs(Definitions.ToDictionary(p => p.Key, p => p.Value.Apply()));
	}
}