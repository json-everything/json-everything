using System.Text.Json;

namespace Json.Patch
{
	internal class PatchContext
	{
		public JsonElement Source { get; set; }
		public string Message { get; set; }
	}
}
