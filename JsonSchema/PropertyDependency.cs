using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
			.ToList();
		if (byKey.Count != Schemas.Count) return false;

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
}

internal class PropertyDependencyJsonConverter : JsonConverter<PropertyDependency>
{
	public override PropertyDependency? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schemas = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options);

		return new PropertyDependency(schemas!);
	}

	public override void Write(Utf8JsonWriter writer, PropertyDependency value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Schemas, options);
	}
}