namespace Json.Patch
{
	public class TestOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new TestOperationHandler();

		private TestOperationHandler() { }

		public void Process(PatchContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}