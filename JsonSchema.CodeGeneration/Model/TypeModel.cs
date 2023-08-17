using System;

namespace Json.Schema.CodeGeneration.Model;

public class TypeModel : IEquatable<TypeModel>
{
	//public List<TypeModel>? DerivedFrom { get; private protected set; }

	//public bool IsGeneric { get; private protected set; }
	//public List<TypeModel>? TypeArguments { get; private protected set; }

	public string? Name { get; }  // used as identifier

	internal bool IsSimple { get; }

	protected TypeModel(){}
	protected TypeModel(string? name)
	{
		Name = name;
	}
	private TypeModel(string name, bool isSimple)
	{
		Name = name;
		IsSimple = isSimple;
	}

	internal static TypeModel Simple(string name)
	{
		return new TypeModel(name, true);
	}

	public virtual bool Equals(TypeModel? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (GetType() != other.GetType()) return false;
		return Name == other.Name && IsSimple == other.IsSimple;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as TypeModel);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ IsSimple.GetHashCode();
		}
	}
}