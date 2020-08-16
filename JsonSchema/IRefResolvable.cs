namespace Json.Schema
{
	public interface IRefResolvable
	{
		IRefResolvable ResolvePointerSegment(string value);
	}
}