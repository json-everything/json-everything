namespace Json.Patch
{
	public class RemoveOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new RemoveOperationHandler();

		private RemoveOperationHandler() { }

		public void Process(PatchContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}