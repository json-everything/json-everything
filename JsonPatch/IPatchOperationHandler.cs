namespace Json.Patch
{
	internal interface IPatchOperationHandler
	{
		void Process(PatchContext context, PatchOperation operation);
	}
}