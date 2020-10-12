namespace Json.Patch
{
	public class MoveOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new MoveOperationHandler();

		private MoveOperationHandler() { }

		public void Process(PatchContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}