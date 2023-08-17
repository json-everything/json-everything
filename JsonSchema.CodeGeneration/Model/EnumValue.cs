namespace Json.Schema.CodeGeneration.Model;

public record EnumValue(string Name, int Value)
{
	public string Name { get; } = Name;
	public int Value { get; } = Value;
}