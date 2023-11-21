using System;

namespace Json.Schema;

/// <summary>
/// Indicates a keyword from which the decorated keyword requires annotations.
/// </summary>
/// <summary>
/// Apply this attribute to your schema keyword to indicate a dependency on another keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class DependsOnAnnotationsFromAttribute : Attribute
{
	/// <summary>
	/// The dependent type.
	/// </summary>
	public Type DependentType { get; }

	/// <summary>
	/// Creates a new <see cref="DependsOnAnnotationsFromAttribute"/> instance.
	/// </summary>
	/// <param name="type">The dependent type.</param>
	public DependsOnAnnotationsFromAttribute(Type type)
	{
		DependentType = type ?? throw new ArgumentNullException(nameof(type));
	}
}

/// <summary>
/// Indicates a keyword from which the decorated keyword requires annotations.
/// </summary>
/// <summary>
/// Apply this attribute to your schema keyword to indicate a dependency on another keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class DependsOnAnnotationsFromAttribute<T> : DependsOnAnnotationsFromAttribute
	where T : IJsonSchemaKeyword
{
	/// <summary>
	/// Creates a new <see cref="DependsOnAnnotationsFromAttribute"/> instance.
	/// </summary>
	public DependsOnAnnotationsFromAttribute()
		: base(typeof(T))
	{
	}
}