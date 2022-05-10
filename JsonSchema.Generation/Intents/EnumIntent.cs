using System.Collections.Generic;
using System.Linq;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `enum` keyword.
/// </summary>
public class EnumIntent : ISchemaKeywordIntent
{
	/// <summary>
	/// The names defined by the enumeration.
	/// </summary>
	public List<string> Names { get; set; }

	/// <summary>
	/// Creates a new <see cref="EnumIntent"/> instance.
	/// </summary>
	/// <param name="names">The names defined by the enumeration.</param>
	public EnumIntent(IEnumerable<string> names)
	{
		Names = names.ToList();
	}

	/// <summary>
	/// Creates a new <see cref="EnumIntent"/> instance.
	/// </summary>
	/// <param name="names">The names defined by the enumeration.</param>
	public EnumIntent(params string[] names)
	{
		Names = names.ToList();
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Enum(Names.Select(n => n.AsJsonElement()));
	}
}