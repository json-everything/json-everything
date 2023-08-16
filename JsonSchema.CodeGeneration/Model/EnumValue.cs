namespace Json.Schema.CodeGeneration.Model;

public class EnumValue
{
	public string Name { get; }
	public int Value { get; }

	public EnumValue(string name, int value)
	{
		Name = name;
		Value = value;
	}
}