using Json.Patch;
#pragma warning disable 8618

namespace TryJsonEverything.Models
{
	public class PatchGenerationOutput
	{
		public JsonPatch Patch { get; init; }
		public string Error { get; set; }
	}
}