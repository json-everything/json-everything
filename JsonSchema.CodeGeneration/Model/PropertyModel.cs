namespace Json.Schema.CodeGeneration.Model;

internal class PropertyModel
{
	public string Name { get; }
	public TypeModel Type { get; set; }
	public bool CanRead { get; }
	public bool CanWrite { get; }

	public PropertyModel(string name, TypeModel type, bool canRead, bool canWrite)
	{
		Name = name;
		Type = type;
		CanRead = canRead;
		CanWrite = canWrite;
	}
}