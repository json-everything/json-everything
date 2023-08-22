using System.Linq;
using System;

namespace Json.Schema.CodeGeneration.Model;

internal class DictionaryModel : TypeModel
{
	public TypeModel Keys { get; private set; }
	public TypeModel Items { get; private set; }

	public DictionaryModel(string? name, TypeModel items)
		: base(name)
	{
		Keys = CommonModels.String;
		Items = items;
	}

	internal override void FillPlaceholders(GenerationCache cache)
	{
		if (Keys is PlaceholderModel keys)
		{
			var entry = cache.FirstOrDefault(x => x.Id == keys.Id);
			if (entry == null)
				throw new ArgumentOutOfRangeException(nameof(cache), "Placeholder was generated but not stored");

			Keys = entry.Model;
		}

		if (Items is PlaceholderModel items)
		{
			var entry = cache.FirstOrDefault(x => x.Id == items.Id);
			if (entry == null)
				throw new ArgumentOutOfRangeException(nameof(cache), "Placeholder was generated but not stored");

			Items = entry.Model;
		}
	}

	public override bool Equals(TypeModel? other)
	{
		if (!base.Equals(other)) return false;

		var dictModel = (DictionaryModel)other;
		return Equals(Keys, dictModel.Keys) && Equals(Items, dictModel.Items);
	}
}