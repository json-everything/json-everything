namespace Json.Schema.CodeGeneration.Model;

internal class ArrayModel : TypeModel
{
	public TypeModel Items { get; }

	public ArrayModel(string? name, TypeModel items)
		: base(name)
	{
		Items = items;
	}

	public override bool Equals(TypeModel? other)
	{
		if (!base.Equals(other)) return false;

		var arrModel = (ArrayModel)other;
		return Equals(Items, arrModel.Items);
	}
}