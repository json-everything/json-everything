namespace Json.Patch
{
	internal class CopyOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new CopyOperationHandler();

		private CopyOperationHandler() { }

		public void Process(PatchContext context, PatchOperation operation)
		{
			throw new System.NotImplementedException();
		}
	}
}