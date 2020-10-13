namespace Json.Patch
{
	internal class TestOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new TestOperationHandler();

		private TestOperationHandler() { }

		public void Process(PatchContext context, PatchOperation operation)
		{
			throw new System.NotImplementedException();
		}
	}
}