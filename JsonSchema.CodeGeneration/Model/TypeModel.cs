using System;

namespace Json.Schema.CodeGeneration.Model;

/// <summary>
/// Base class for modeling a type.
/// </summary>
public class TypeModel : IEquatable<TypeModel>
{
	//public List<TypeModel>? DerivedFrom { get; private protected set; }

	//public bool IsGeneric { get; private protected set; }
	//public List<TypeModel>? TypeArguments { get; private protected set; }

	/// <summary>
	/// Gets the name of the type.  Provided by the `title` keyword.
	/// </summary>
	public string? Name { get; }  // used as identifier

	internal bool IsSimple { get; }

	private protected TypeModel(string? name)
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

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public virtual bool Equals(TypeModel? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (GetType() != other.GetType()) return false;
		return Name == other.Name && IsSimple == other.IsSimple;
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object? obj)
	{
		return Equals(obj as TypeModel);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ IsSimple.GetHashCode();
		}
	}
}