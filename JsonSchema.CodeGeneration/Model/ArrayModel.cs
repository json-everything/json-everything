namespace Json.Schema.CodeGeneration.Model;

public class ArrayModel : TypeModel
{
	public TypeModel Items { get; }

	public ArrayModel(TypeModel items)
	{
		Items = items;
	}
}