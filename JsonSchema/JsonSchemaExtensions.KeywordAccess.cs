namespace Json.Schema;

/// <summary>
/// Some extensions for <see cref="JsonSchema"/>
/// </summary>
public static partial class JsonSchemaExtensions
{
	public static string? GetTitle(this JsonSchema schema)
	{
		return schema.TryGetKeyword<TitleKeyword>(TitleKeyword.Name, out var k) ? k!.Value : null;
	}

	public static string? GetDescription(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DescriptionKeyword>(DescriptionKeyword.Name, out var k) ? k!.Value : null;
	}
}