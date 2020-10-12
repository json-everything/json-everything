namespace Json.Patch
{
	public class AddOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new AddOperationHandler();

		private AddOperationHandler(){}

		public void Process(PatchContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}