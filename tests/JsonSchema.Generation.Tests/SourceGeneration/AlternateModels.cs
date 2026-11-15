using Json.Schema.Generation.Serialization;

namespace Json.Schema.Generation.Tests.SourceGeneration.AlternateNamespace;

public class AlternateModels
{
	[GenerateJsonSchema]
	public class CrossNamespacePerson
	{
		public string Name { get; set; } = string.Empty;
		public int Age { get; set; }
	}
}

/// <summary>
/// Outer class with the same name as <c>TestModels</c> in the main namespace,
/// so that nested <c>ConflictModel</c> triggers name de-duplication.
/// </summary>
public class TestModels
{
	[GenerateJsonSchema]
	public class ConflictModel
	{
		public int Count { get; set; }
	}
}
