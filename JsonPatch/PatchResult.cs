using System.Text.Json;

namespace Json.Patch
{
	public class PatchResult
	{
		public JsonElement Result { get; }
		public string Error { get; }

		public bool IsSuccess => Error == null;

		internal PatchResult(PatchContext context)
		{
			Result = context.Source;
			Error = context.Message;
		}
	}
}