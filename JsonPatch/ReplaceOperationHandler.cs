namespace Json.Patch
{
	public class ReplaceOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new ReplaceOperationHandler();

		private ReplaceOperationHandler() { }

		public void Process(PatchContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}