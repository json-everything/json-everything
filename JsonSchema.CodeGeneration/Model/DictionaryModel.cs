namespace Json.Schema.CodeGeneration.Model;

public class DictionaryModel : TypeModel
{
	public TypeModel Keys { get; } // could use propertyNames to declare a key type
	public TypeModel Items { get; }

	public DictionaryModel(TypeModel items)
	{
		Keys = CommonModels.String;
		Items = items;
	}
}