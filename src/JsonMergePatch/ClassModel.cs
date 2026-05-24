using System.Collections.Immutable;

namespace Json.MergePatch;

internal sealed class ClassModel
{
	public string? Namespace { get; }
	public string ClassName { get; }
	public string FullName { get; }
	public string PatchTypeName { get; }
	public string EmittedPatchTypeName { get; }
	public bool GenerateAsNested { get; }
	public ImmutableArray<PropertyModel> Properties { get; }

	public ClassModel(
		string? @namespace,
		string className,
		string fullName,
		string patchTypeName,
		string emittedPatchTypeName,
		bool generateAsNested,
		ImmutableArray<PropertyModel> properties)
	{
		Namespace = @namespace;
		ClassName = className;
		FullName = fullName;
		PatchTypeName = patchTypeName;
		EmittedPatchTypeName = emittedPatchTypeName;
		GenerateAsNested = generateAsNested;
		Properties = properties;
	}
}