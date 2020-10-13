namespace Json.Patch
{
	internal class ReplaceOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new ReplaceOperationHandler();

		private ReplaceOperationHandler() { }

		public void Process(PatchContext context, PatchOperation operation)
		{
			throw new System.NotImplementedException();
		}
	}
}