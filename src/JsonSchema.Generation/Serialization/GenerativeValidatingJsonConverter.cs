using System;
using System.Linq;
using System.Reflection;
using Json.Schema.Serialization;

namespace Json.Schema.Generation.Serialization;

/// <summary>
/// Extends <see cref="ValidatingJsonConverter"/> to also allow for
/// schema generation using <see cref="GenerateJsonSchemaAttribute"/>.
/// </summary>
public class GenerativeValidatingJsonConverter : ValidatingJsonConverter
{
	/// <summary>
	/// Provides options for the generator.
	/// </summary>
	public SchemaGeneratorConfiguration GeneratorConfiguration { get; } = new();

	/// <summary>When overridden in a derived class, determines whether the converter instance can convert the specified object type.</summary>
	/// <param name="typeToConvert">The type of the object to check whether it can be converted by this converter instance.</param>
	/// <returns>
	/// <see langword="true" /> if the instance can convert the specified object type; otherwise, <see langword="false" />.</returns>
	public override bool CanConvert(Type typeToConvert)
	{
		var canConvert = typeToConvert.GetCustomAttributes(typeof(GenerateJsonSchemaAttribute)).SingleOrDefault() != null;

		return canConvert || base.CanConvert(typeToConvert);
	}

	/// <summary>
	/// Gets the schema for a type.
	/// </summary>
	protected override JsonSchema GetSchema(Type type)
	{
		var generateAttribute = type.GetCustomAttributes(typeof(GenerateJsonSchemaAttribute)).SingleOrDefault();
		if (generateAttribute is not null)
		{
			var schema = new JsonSchemaBuilder().FromType(type, GeneratorConfiguration);
			return schema;
		}

		return base.GetSchema(type);
	}
}