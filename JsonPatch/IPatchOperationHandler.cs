namespace Json.Patch
{
	public interface IPatchOperationHandler
	{
		void Process(PatchContext context);
	}
}