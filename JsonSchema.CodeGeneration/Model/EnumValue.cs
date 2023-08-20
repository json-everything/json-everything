namespace Json.Schema.CodeGeneration.Model;

internal record EnumValue(string Name, int Value)
{
	public string Name { get; } = Name;
	public int Value { get; } = Value;
}