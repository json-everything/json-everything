using Json.More;

namespace Json.Patch
{
	internal class TestOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new TestOperationHandler();

		private TestOperationHandler() { }

		public void Process(PatchContext context, PatchOperation operation)
		{
			var current = context.Source;
			var message = EditableJsonElementHelpers.FindTarget(ref current, operation.Path);

			if (message != null)
			{
				context.Message = message;
				return;
			}

			if (current.ToElement().IsEquivalentTo(operation.Value)) return;

			context.Message = $"Path `{operation.Path}` is not equal to the indicated value.";
		}
	}
}