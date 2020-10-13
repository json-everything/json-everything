namespace Json.Patch
{
	internal class MoveOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new MoveOperationHandler();

		private MoveOperationHandler() { }

		public void Process(PatchContext context, PatchOperation operation)
		{
			throw new System.NotImplementedException();
		}
	}
}