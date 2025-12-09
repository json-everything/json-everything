using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

/// <summary>
/// Represents either a JSON schema definition or a list of required property names for use in schema composition and
/// validation scenarios.
/// </summary>
/// <remarks>This type will not need to be interacted with directly. Use implicit conversion from a <see cref="JsonSchemaBuilder"/>
/// to represent a schema, or from a list or array of strings to represent required properties.</remarks>
public class SchemaOrPropertyList
{
	internal JsonSchemaBuilder? Schema { get; }
	internal string[]? Requirements { get; }

	private SchemaOrPropertyList(JsonSchemaBuilder schema)
	{
		Schema = schema;
	}

	private SchemaOrPropertyList(IEnumerable<string> requirements)
	{
		Requirements = requirements.ToArray();
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