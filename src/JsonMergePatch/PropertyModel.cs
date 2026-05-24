namespace Json.MergePatch;

internal sealed class PropertyModel
{
	public string CSharpName { get; }
	public string JsonName { get; }
	public string InnerTypeName { get; }
	public bool IsPatchModel { get; }
	public bool IsNullable { get; }
	public string? ConcreteTypeName { get; }

	public PropertyModel(
		string cSharpName,
		string jsonName,
		string innerTypeName,
		bool isPatchModel,
		bool isNullable,
		string? concreteTypeName)
	{
		CSharpName = cSharpName;
		JsonName = jsonName;
		InnerTypeName = innerTypeName;
		IsPatchModel = isPatchModel;
		IsNullable = isNullable;
		ConcreteTypeName = concreteTypeName;
	}
}