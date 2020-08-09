namespace Json.Schema
{
	public interface IJsonSchemaKeyword
	{
		ValidationResults Validate(ValidationContext context);
	}
}