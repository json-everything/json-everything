using Json.More;

namespace Json.Patch;

internal class TestOperationHandler : IPatchOperationHandler
{
	public static IPatchOperationHandler Instance { get; } = new TestOperationHandler();

	private TestOperationHandler() { }

	public void Process(PatchContext context, PatchOperation operation)
	{
		if (!operation.Path.TryEvaluate(context.Source, out var data))
		{
			context.Message = $"Path `{operation.Path}` could not be reached.";
			return;
		}

		if (data.IsEquivalentTo(operation.Value)) return;
		
		context.Message = $"Path `{operation.Path}` is not equal to the indicated value.";
	}
}