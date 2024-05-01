using System;
using System.Linq.Expressions;

namespace Json.Pointer;

/// <summary>
/// Options for creating pointers using <see cref="JsonPointer.Create{T}(Expression{Func{T, object}}, PointerCreationOptions)"/>.
/// </summary>
public class PointerCreationOptions
{
	private PropertyNameResolver? _propertyNameResolver;
	
	/// <summary>
	/// Default settings.
	/// </summary>
	public static readonly PointerCreationOptions Default = new();

	/// <summary>
	/// Gets or sets the property naming resolver.  Default is <see cref="PropertyNameResolvers.AsDeclared"/>.
	/// </summary>
	public PropertyNameResolver PropertyNameResolver
	{
		get => _propertyNameResolver ??= PropertyNameResolvers.AsDeclared;
		set => _propertyNameResolver = value;
	}
}