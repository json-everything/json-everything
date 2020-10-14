using System.Text.Json;

namespace Json.Patch
{
	public class PatchResult
	{
		public JsonElement Result { get; }
		public string Error { get; }
		public int Operation { get; }

		public bool IsSuccess => Error == null;

		internal PatchResult(PatchContext context)
		{
			Result = context.Source.ToElement();
			Error = context.Message;
			Operation = context.Index;
		}
	}
}