namespace Json.Schema.CodeGeneration.Model;

public class TypeModel
{
	//public List<TypeModel>? DerivedFrom { get; private protected set; }

	//public bool IsGeneric { get; private protected set; }
	//public List<TypeModel>? TypeArguments { get; private protected set; }

	public string? Name { get; set; }  // used as identifier

	internal bool IsSimple { get; private set; }

	protected TypeModel(){}

	internal static TypeModel Simple(string name)
	{
		return new TypeModel
		{
			Name = name,
			IsSimple = true
		};
	}
}