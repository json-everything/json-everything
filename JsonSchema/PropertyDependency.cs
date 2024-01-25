using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Represents a single entry in the `propertyDependencies` keyword.
/// </summary>
[JsonConverter(typeof(PropertyDependencyJsonConverter))]
public class PropertyDependency : IKeyedSchemaCollector, IEquatable<PropertyDependency>
{
	/// <summary>
	/// Gets the collection of value-dependent schemas for this property.
	/// </summary>
	public IReadOnlyDictionary<string, JsonSchema> Schemas { get; }

	/// <summary>
	/// Creates a new instance of <see cref="PropertyDependency"/>.
	/// </summary>
	/// <param name="schemas">The collection of value-dependent schemas for this property</param>
	public PropertyDependency(IReadOnlyDictionary<string, JsonSchema> schemas)
	{
		Schemas = schemas;
	}

	/// <summary>
	/// Creates a new instance of <see cref="PropertyDependency"/>.
	/// </summary>
	/// <param name="schemas">The collection of value-dependent schemas for this property</param>
	public PropertyDependency(params (string property, JsonSchema schema)[] schemas)
	{
		Schemas = schemas.ToDictionary(x => x.property, x => x.schema);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(PropertyDependency? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (Schemas.Count != other.Schemas.Count) return false;
		var byKey = Schemas.Join(other.Schemas,
				td => td.Key,
				od => od.Key,
				(td, od) => new { ThisDef = td.Value, OtherDef = od.Value })
			.ToArray();
		if (byKey.Length != Schemas.Count) return false;

		return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object? obj)
	{
		return Equals(obj as PropertyDependency);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Schemas.GetStringDictionaryHashCode();
	}

	/// <summary>
	/// Implicitly converts a keyed collection of <see cref="JsonSchema"/> to a property dependency.
	/// </summary>
	/// <param name="dependency"></param>
	public static implicit operator PropertyDependency(Dictionary<string, JsonSchema> dependency)
	{
		return new PropertyDependency(dependency);
	}

	/// <summary>
	/// Implicitly converts a keyed collection of <see cref="JsonSchema"/> to a property dependency.
	/// </summary>
	/// <param name="dependency"></param>
	public static implicit operator PropertyDependency((string property, JsonSchema schema)[] dependency)
	{
		return new PropertyDependency(dependency);
	}

	/// <summary>
	/// Implicitly converts a keyed collection of <see cref="JsonSchema"/> to a property dependency.
	/// </summary>
	/// <param name="dependency"></param>
	public static implicit operator PropertyDependency((string property, JsonSchemaBuilder schema)[] dependency)
	{
		return new PropertyDependency(dependency.ToDictionary(x => x.property, x => x.schema.Build()));
	}
}

/// <summary>
/// JSON converter for <see cref="PropertyDependency"/>.
/// </summary>
public sealed class PropertyDependencyJsonConverter : JsonConverter<PropertyDependency>
{
	/// <summary>Reads and converts the JSON to type <see cref="PropertyDependency"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override PropertyDependency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schemas = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.DictionaryStringJsonSchema);

		return new PropertyDependency(schemas!);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, PropertyDependency value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Schemas, options);
	}
}