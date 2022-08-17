using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

[JsonConverter(typeof(PropertyDependencyJsonConverter))]
public class PropertyDependency : IKeyedSchemaCollector, IEquatable<PropertyDependency>
{
	public IReadOnlyDictionary<string, JsonSchema> Schemas { get; }

	public PropertyDependency(IReadOnlyDictionary<string, JsonSchema> schemas)
	{
		Schemas = schemas;
	}

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

	public override bool Equals(object? obj)
	{
		return Equals(obj as PropertyDependency);
	}

	public override int GetHashCode()
	{
		return Schemas.GetStringDictionaryHashCode();
	}
}

internal class PropertyDependencyJsonConverter : JsonConverter<PropertyDependency>
{
	public override PropertyDependency? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, PropertyDependency value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}