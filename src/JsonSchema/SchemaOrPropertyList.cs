using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Schema;

public class SchemaOrPropertyList
{
	internal JsonSchemaBuilder? Schema { get; }
	internal JsonArray? Requirements { get; }

	private SchemaOrPropertyList(JsonSchemaBuilder schema)
	{
		Schema = schema;
	}

	private SchemaOrPropertyList(IEnumerable<string> requirements)
	{
		Requirements = new JsonArray(requirements.Select(x => (JsonNode?)x).ToArray());
	}

	/// <summary>
	/// Implicitly creates a <see cref="SchemaOrPropertyList"/> from a <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	public static implicit operator SchemaOrPropertyList(JsonSchemaBuilder schema)
	{
		return new SchemaOrPropertyList(schema);
	}

	/// <summary>
	/// Implicitly creates a <see cref="SchemaOrPropertyList"/> from a list of strings.
	/// </summary>
	public static implicit operator SchemaOrPropertyList(List<string> requirements)
	{
		return new SchemaOrPropertyList(requirements);
	}

	/// <summary>
	/// Implicitly creates a <see cref="SchemaOrPropertyList"/> from an array of strings.
	/// </summary>
	public static implicit operator SchemaOrPropertyList(string[] requirements)
	{
		return new SchemaOrPropertyList(requirements);
	}
}