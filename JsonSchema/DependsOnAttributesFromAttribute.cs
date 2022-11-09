using System;

namespace Json.Schema;

/// <summary>
/// Indicates a keyword from which the decorated keyword requires annotations.
/// </summary>
/// <summary>
/// Apply this attribute to your schema keyword to indicate a dependency on another keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class DependsOnAttributesFromAttribute : Attribute
{
	/// <summary>
	/// The dependent type.
	/// </summary>
	public Type DependentType { get; }

	/// <summary>
	/// Creates a new <see cref="DependsOnAttributesFromAttribute"/> instance.
	/// </summary>
	/// <param name="type">The dependent type.</param>
	public DependsOnAttributesFromAttribute(Type type)
	{
		DependentType = type ?? throw new ArgumentNullException(nameof(type));
	}
}