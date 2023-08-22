using System;
using System.Linq;

namespace Json.Schema.CodeGeneration.Model;

internal class ArrayModel : TypeModel
{
	public TypeModel Items { get; private set; }

	public ArrayModel(string? name, TypeModel items)
		: base(name)
	{
		Items = items;
	}

	internal override void FillPlaceholders(GenerationCache cache)
	{
		if (Items is not PlaceholderModel placeholder) return;

		var entry = cache.FirstOrDefault(x => x.Id == placeholder.Id);
		if (entry == null)
			throw new ArgumentOutOfRangeException(nameof(cache), "Placeholder was generated but not stored");

		Items = entry.Model;
	}

	public override bool Equals(TypeModel? other)
	{
		if (!base.Equals(other)) return false;

		var arrModel = (ArrayModel)other;
		return Equals(Items, arrModel.Items);
	}
}