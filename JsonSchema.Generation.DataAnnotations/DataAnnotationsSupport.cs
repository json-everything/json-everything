namespace Json.Schema.Generation.DataAnnotations;

public static class DataAnnotationsSupport
{
	public static void AddDataAnnotations()
	{
		AttributeHandler.AddHandler<RangeAttributeHandler>();
	}
}