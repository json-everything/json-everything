using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Schema.Generation.Intents;

/// <summary>
/// Provides intent to create an `enum` keyword.
/// </summary>
public class EnumIntent : ISchemaKeywordIntent
{
	private readonly List<JsonNode?>? _values;

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
		Names = [.. names];
	}

	/// <summary>
	/// Creates a new <see cref="EnumIntent"/> instance.
	/// </summary>
	/// <param name="names">The names defined by the enumeration.</param>
	public EnumIntent(params string[] names)
	{
		Names = [.. names];
	}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public EnumIntent(IEnumerable<JsonNode> values)
	{
		_values = [.. values];
	}

	public EnumIntent(params JsonNode?[] values)
	{
		_values = [.. values];
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The builder.</param>
	public void Apply(JsonSchemaBuilder builder)
	{
		builder.Enum(_values ?? Names.Select(n => (JsonNode?)n));
	}

	internal void AddValue(JsonNode? value)
	{
		_values?.Add(value);
	}
}