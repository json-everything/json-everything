using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.CodeGeneration.Model;

internal class ObjectModel : TypeModel
{
	public PropertyModel[] Properties { get; }

	public ObjectModel(string name, IEnumerable<PropertyModel> properties)
		: base(name)
	{
		Properties = properties.ToArray();
	}

	internal override void FillPlaceholders(GenerationCache cache)
	{
		foreach (var property in Properties)
		{
			if (property.Type is not PlaceholderModel placeholder) continue;

			var entry = cache.FirstOrDefault(x => x.Id == placeholder.Id);
			if (entry == null)
				throw new ArgumentOutOfRangeException(nameof(cache), "Placeholder was generated but not stored");

			property.Type = entry.Model;
		}
	}

	public override bool Equals(TypeModel? other)
	{
		if (!base.Equals(other)) return false;

		var objModel = (ObjectModel)other;

		if (Properties.Length != objModel.Properties.Length) return false;
		if (Properties.Select(x => x.Name).Except(objModel.Properties.Select(x => x.Name)).Any()) return false;

		var bothProperties = Properties.Join(objModel.Properties,
			x => x.Name,
			y => y.Name,
			(x, y) => (x, y));

		foreach (var both in bothProperties)
		{
			if (both.x.CanRead != both.y.CanRead) return false;
			if (both.x.CanWrite != both.y.CanWrite) return false;
			if (!Equals(both.x.Type, both.y.Type)) return false;
		}

		return true;
	}
}