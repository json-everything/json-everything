namespace Json.Patch
{
	public class CopyOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new CopyOperationHandler();

		private CopyOperationHandler() { }

		public void Process(PatchContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}