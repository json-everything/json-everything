namespace Json.Patch
{
	internal class RemoveOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new RemoveOperationHandler();

		private RemoveOperationHandler() { }

		public void Process(PatchContext context, PatchOperation operation)
		{
			throw new System.NotImplementedException();
		}
	}
}