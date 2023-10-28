namespace Json.Pointer;

public class PointerCreationOptions
{
	private PropertyNameResolver? _propertyNameResolver;
	
	public static PointerCreationOptions Default = new();

	public PropertyNameResolver? PropertyNameResolver
	{
		get => _propertyNameResolver ??= PropertyNameResolvers.AsDeclared;
		set => _propertyNameResolver = value;
	}
}