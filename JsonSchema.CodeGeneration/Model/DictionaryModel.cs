namespace Json.Schema.CodeGeneration.Model;

public class DictionaryModel : TypeModel
{
	public TypeModel Keys { get; } // could use propertyNames to declare a key type
	public TypeModel Items { get; }

	public DictionaryModel(string? name, TypeModel items)
		: base(name)
	{
		Keys = CommonModels.String;
		Items = items;
	}

	public override bool Equals(TypeModel? other)
	{
		if (!base.Equals(other)) return false;

		var dictModel = (DictionaryModel)other;
		return Equals(Keys, dictModel.Keys) && Equals(Items, dictModel.Items);
	}
}