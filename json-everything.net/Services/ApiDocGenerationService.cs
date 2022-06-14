namespace JsonEverythingNet.Services;

public class ApiDocGenerationService
{
	public string GenerateForType(Type type)
	{
		return $"# Look at me!\n\nI'm `{type.CSharpName()}` API docs!";
	}
}